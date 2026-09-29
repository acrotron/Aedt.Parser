using AwesomeAssertions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser.Tests;

[TestClass]
public class BoundingBoxCollectionTests
{
    [TestMethod]
    public void Polygon_EmptyCollection_Throws()
    {
        var collection = new BoundingBoxCollection();

        Action act = () => collection.Polygon();

        act.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void Polygon_SingleValidBox_ReturnsPolygon()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(10, 0));
        bbox.Coordinates.Add(new Coordinate(10, 10));
        bbox.Coordinates.Add(new Coordinate(0, 10));

        var collection = new BoundingBoxCollection { bbox };

        Polygon polygon = collection.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().Be(100.0);
    }

    [TestMethod]
    public void Polygon_MultipleOverlappingBoxes_ReturnsUnion()
    {
        // Two overlapping squares: (0,0)-(10,10) and (5,0)-(15,10)
        var bbox1 = new BoundingBox();
        bbox1.Coordinates.Add(new Coordinate(0, 0));
        bbox1.Coordinates.Add(new Coordinate(10, 0));
        bbox1.Coordinates.Add(new Coordinate(10, 10));
        bbox1.Coordinates.Add(new Coordinate(0, 10));

        var bbox2 = new BoundingBox();
        bbox2.Coordinates.Add(new Coordinate(5, 0));
        bbox2.Coordinates.Add(new Coordinate(15, 0));
        bbox2.Coordinates.Add(new Coordinate(15, 10));
        bbox2.Coordinates.Add(new Coordinate(5, 10));

        var collection = new BoundingBoxCollection { bbox1, bbox2 };

        Polygon polygon = collection.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        // Union of two 10x10 squares overlapping by 5x10 = 150
        Math.Abs(polygon.Area - 150.0).Should().BeLessThan(0.01);
    }

    [TestMethod]
    public void Polygon_MultipleNonOverlappingBoxes_ReturnsLargest()
    {
        // Small box: area 1
        var small = new BoundingBox();
        small.Coordinates.Add(new Coordinate(100, 100));
        small.Coordinates.Add(new Coordinate(101, 100));
        small.Coordinates.Add(new Coordinate(101, 101));
        small.Coordinates.Add(new Coordinate(100, 101));

        // Large box: area 100
        var large = new BoundingBox();
        large.Coordinates.Add(new Coordinate(0, 0));
        large.Coordinates.Add(new Coordinate(10, 0));
        large.Coordinates.Add(new Coordinate(10, 10));
        large.Coordinates.Add(new Coordinate(0, 10));

        var collection = new BoundingBoxCollection { small, large };

        Polygon polygon = collection.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().Be(100.0);
    }

    [TestMethod]
    public void Polygon_SelfIntersectingBox_ReturnsLargestLobe()
    {
        // A bowtie/figure-8 shape: self-intersecting at the center, two triangular lobes of area 25 that only touch
        // at (5, 5). A single polygon can't hold both, so the largest lobe is returned.
        var collection = new BoundingBoxCollection { CreateBowtie() };

        Polygon polygon = collection.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().BeApproximately(25.0, 1e-9);
    }

    [TestMethod]
    public void Polygon_SelfIntersectingBoxJoinedByOtherBox_KeepsBothLobes()
    {
        // Arrange - the bowtie's lobes are (0,0)-(0,10)-(5,5) and (10,0)-(10,10)-(5,5); a band across the middle
        // connects them, so the union must contain both lobes.
        var band = new BoundingBox();
        band.Coordinates.Add(new Coordinate(0, 4));
        band.Coordinates.Add(new Coordinate(10, 4));
        band.Coordinates.Add(new Coordinate(10, 6));
        band.Coordinates.Add(new Coordinate(0, 6));

        var collection = new BoundingBoxCollection { CreateBowtie(), band };

        var factory = new GeometryFactory();
        Geometry expected = factory.CreatePolygon([new(0, 0), new(0, 10), new(5, 5), new(0, 0)])
            .Union(factory.CreatePolygon([new(10, 0), new(10, 10), new(5, 5), new(10, 0)]))
            .Union(band.Polygon());

        // Act
        Polygon polygon = collection.Polygon();

        // Assert
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().BeApproximately(expected.Area, 1e-9);
    }

    [TestMethod]
    public void Polygon_OnlyDegenerateBox_Throws()
    {
        // Arrange - collinear points have no area
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(5, 5));
        bbox.Coordinates.Add(new Coordinate(10, 10));

        var collection = new BoundingBoxCollection { bbox };

        // Act
        Action act = () => collection.Polygon();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static BoundingBox CreateBowtie()
    {
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(10, 10));
        bbox.Coordinates.Add(new Coordinate(10, 0));
        bbox.Coordinates.Add(new Coordinate(0, 10));
        return bbox;
    }

    [TestMethod]
    [DeploymentItem("DGBBoxes_J10.csv")]
    public void Polygon_FromFile_ReturnsValidPolygon()
    {
        var reader = new BoundingBoxReader();
        BoundingBoxCollection collection = reader.ReadBoundingBoxes("DGBBoxes_J10.csv");

        Polygon polygon = collection.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().BeGreaterThan(0);
    }
}