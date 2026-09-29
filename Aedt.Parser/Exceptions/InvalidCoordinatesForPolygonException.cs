namespace Aedt.Parser.Exceptions;

/// <summary>
/// Thrown when the coordinates of a bounding box cannot form a polygon (e.g. fewer than three distinct points).
/// </summary>
/// <param name="ex">The underlying geometry exception.</param>
public class InvalidCoordinatesForPolygonException(Exception ex) : Exception("Couldn't create polygon from provided coordinates.", ex)
{
}
