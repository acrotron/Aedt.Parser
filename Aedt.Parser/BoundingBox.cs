using Aedt.Parser.Exceptions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser;

/// <summary>
/// Represents a bounding box defined by a collection of coordinates and provides functionality to generate a polygon
/// from those coordinates.
/// </summary>
public sealed class BoundingBox
{
    /// <summary>
    /// Gets the list of coordinates associated with the object.
    /// </summary>
    public List<Coordinate> Coordinates { get; } = [];

    /// <summary>
    /// Creates a polygon from the bounding box coordinates.
    /// </summary>
    /// <returns>a Polygon based on the bounding box coordinates</returns>
    /// <exception cref="NoCoordinatesFoundException"></exception>
    /// <exception cref="InvalidCoordinatesForPolygonException"></exception>
    public Polygon Polygon()
    {
        if (Coordinates.Count == 0) throw new NoCoordinatesFoundException();

        try
        {
            List<Coordinate> coordinates = [..Coordinates];

            // Close the ring if not already closed
            if (!coordinates[0].Equals2D(coordinates[^1]))
            {
                coordinates.Add(coordinates[0]);
            }

            Polygon polygon = new Polygon(new LinearRing(coordinates.ToArray()));

            return polygon;
        }
        catch (Exception ex)
        {
            throw new InvalidCoordinatesForPolygonException(ex);
        }
    }
}