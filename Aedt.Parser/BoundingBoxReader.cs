using Aedt.Parser.Exceptions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser;

/// <summary>
/// Reads AEDT bounding polygon and bounding box CSV files.
/// Coordinates are rounded to 5 decimals (about 1 m).
/// </summary>
public sealed class BoundingBoxReader
{
    /// <summary>
    /// Reads a polygon file (e.g. PRBPolygon) with one <c>longitude, latitude</c> vertex per line.
    /// Blank lines and the <c>END</c> trailer are skipped.
    /// </summary>
    /// <param name="filePath">Path of the CSV file.</param>
    /// <returns>A bounding box with the vertices in file order.</returns>
    /// <exception cref="AedtFormatException">A line does not have exactly two numeric fields.</exception>
    public BoundingBox ReadPolygon(string filePath)
    {
        BoundingBox boundingBox = new BoundingBox();
        using StreamReader reader = new StreamReader(filePath);

        int lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (AedtCsvLine.IsSkipped(line)) continue;

            string[] values = AedtCsvLine.Split(line);

            if (values.Length != 2)
            {
                throw new AedtFormatException(filePath, lineNumber, line,
                    $"Expected 2 fields (longitude, latitude) but found {values.Length}.");
            }

            boundingBox.Coordinates.Add(ParseCoordinate(values, 0, filePath, lineNumber, line));
        }

        return boundingBox;
    }

    /// <summary>
    /// Reads a bounding box file (e.g. DGBBoxes) with one box per line, given as <c>longitude, latitude</c> pairs.
    /// Blank lines and the <c>END</c> trailer are skipped.
    /// </summary>
    /// <param name="filePath">Path of the CSV file.</param>
    /// <returns>One bounding box per line.</returns>
    /// <exception cref="AedtFormatException">A line has an odd number of fields or a field that is not a number.</exception>
    public BoundingBoxCollection ReadBoundingBoxes(string filePath)
    {
        BoundingBoxCollection boxes = new BoundingBoxCollection();
        using StreamReader reader = new StreamReader(filePath);

        int lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (AedtCsvLine.IsSkipped(line)) continue;

            string[] values = AedtCsvLine.Split(line);

            if (values.Length % 2 != 0)
            {
                throw new AedtFormatException(filePath, lineNumber, line,
                    $"Expected longitude, latitude pairs but found {values.Length} fields.");
            }

            BoundingBox boundingBox = new BoundingBox();

            for (int i = 0; i < values.Length; i += 2)
            {
                boundingBox.Coordinates.Add(ParseCoordinate(values, i, filePath, lineNumber, line));
            }

            boxes.Add(boundingBox);
        }

        return boxes;
    }

    private static Coordinate ParseCoordinate(string[] values, int index, string filePath, int lineNumber, string line)
    {
        double longitude = Math.Round(AedtCsvLine.ParseDouble(values[index], filePath, lineNumber, line), 5);
        double latitude = Math.Round(AedtCsvLine.ParseDouble(values[index + 1], filePath, lineNumber, line), 5);

        return new Coordinate(longitude, latitude);
    }
}
