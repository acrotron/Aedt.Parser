namespace Aedt.Parser.Exceptions;

public class InvalidCoordinatesForPolygonException(Exception ex) : Exception("Couldn't create polygon from provided coordinates.", ex)
{
}