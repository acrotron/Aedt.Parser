using System.Globalization;
using Aedt.Parser.Exceptions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser;

/// <summary>
/// Reads AEDT receptor (noise value) CSV files.
/// </summary>
public sealed class ReceptorsReader
{
    /// <summary>
    /// Reads a receptor file with one <c>longitude, latitude, value</c> line per receptor.
    /// Blank lines and the <c>END</c> trailer are skipped.
    /// </summary>
    /// <param name="filePath">Path of the CSV file.</param>
    /// <returns>
    /// The receptors as X = longitude, Y = latitude, M = value. Longitude and latitude are rounded to 6 decimals
    /// the way Python's <c>float(format(x, '.6f'))</c> does.
    /// </returns>
    /// <exception cref="AedtFormatException">A line does not have exactly three numeric fields.</exception>
    public List<CoordinateM> Read(string filePath)
    {
        var points = new List<CoordinateM>();
        using var reader = new StreamReader(filePath);

        int lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (AedtCsvLine.IsSkipped(line)) continue;

            string[] values = AedtCsvLine.Split(line);

            if (values.Length != 3)
            {
                throw new AedtFormatException(filePath, lineNumber, line,
                    $"Expected 3 fields (longitude, latitude, value) but found {values.Length}.");
            }

            double longitude = RoundLikePython(AedtCsvLine.ParseDouble(values[0], filePath, lineNumber, line));
            double latitude = RoundLikePython(AedtCsvLine.ParseDouble(values[1], filePath, lineNumber, line));
            double value = AedtCsvLine.ParseDouble(values[2], filePath, lineNumber, line);

            points.Add(new CoordinateM(longitude, latitude, value));
        }

        return points;
    }

    /// <summary>
    /// Rounds to 6 decimals exactly like Python's <c>float(format(x, '.6f'))</c>: both round the exact binary value
    /// correctly. <see cref="Math.Round(double, int)"/> differs for values ending in a 5 at the 7th decimal
    /// (e.g. -11.8761555).
    /// </summary>
    private static double RoundLikePython(double x) =>
        double.Parse(x.ToString("F6", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
}
