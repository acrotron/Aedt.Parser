using Aedt.Parser.Exceptions;
using AwesomeAssertions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser.Tests;

[TestClass]
public class BoundingBoxTests
{
    [TestMethod]
    public void Polygon_EmptyCoordinates_Throws()
    {
        var bbox = new BoundingBox();

        Action act = () => bbox.Polygon();

        act.Should().Throw<NoCoordinatesFoundException>();
    }

    [TestMethod]
    public void Polygon_ClosedRing_ReturnsValidPolygon()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(10, 0));
        bbox.Coordinates.Add(new Coordinate(10, 10));
        bbox.Coordinates.Add(new Coordinate(0, 10));
        bbox.Coordinates.Add(new Coordinate(0, 0)); // already closed

        Polygon polygon = bbox.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().Be(100.0);
    }

    [TestMethod]
    public void Polygon_OpenRing_AutoCloses()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(10, 0));
        bbox.Coordinates.Add(new Coordinate(10, 10));
        bbox.Coordinates.Add(new Coordinate(0, 10));
        // not closed — Polygon() should close it

        Polygon polygon = bbox.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().Be(100.0);
        polygon.ExteriorRing.IsClosed.Should().BeTrue();
    }

    [TestMethod]
    public void Polygon_Triangle_ReturnsValidPolygon()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(6, 0));
        bbox.Coordinates.Add(new Coordinate(3, 4));

        Polygon polygon = bbox.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().Be(12.0);
    }

    [TestMethod]
    public void Polygon_TwoPoints_CreatesDegenerate()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(10, 0));

        // NTS accepts degenerate rings — two points auto-closed to 3-coord ring
        Polygon polygon = bbox.Polygon();

        polygon.Should().NotBeNull();
        polygon.Area.Should().Be(0);
    }

    [TestMethod]
    public void Polygon_SinglePoint_Throws()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(5, 5));

        // Single point auto-closed to [A, A] — too few for LinearRing
        Action act = () => bbox.Polygon();

        act.Should().Throw<InvalidCoordinatesForPolygonException>();
    }

    [TestMethod]
    public void Polygon_CollinearPoints_CreatesDegenerate()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(5, 0));
        bbox.Coordinates.Add(new Coordinate(10, 0));

        // Collinear points form a zero-area polygon
        Polygon polygon = bbox.Polygon();

        polygon.Should().NotBeNull();
        polygon.Area.Should().Be(0);
    }
}