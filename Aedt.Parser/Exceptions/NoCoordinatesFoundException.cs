namespace Aedt.Parser.Exceptions;

/// <summary>
/// Thrown when a polygon is requested from a bounding box that has no coordinates.
/// </summary>
public class NoCoordinatesFoundException() : Exception("The bounding box has no coordinates.")
{
}
