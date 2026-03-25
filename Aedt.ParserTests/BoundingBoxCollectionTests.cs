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
    public void Polygon_SelfIntersectingBox_RepairedViaBuffer()
    {
        // A bowtie/figure-8 shape: self-intersecting at the center
        var bbox = new BoundingBox();
        bbox.Coordinates.Add(new Coordinate(0, 0));
        bbox.Coordinates.Add(new Coordinate(10, 10));
        bbox.Coordinates.Add(new Coordinate(10, 0));
        bbox.Coordinates.Add(new Coordinate(0, 10));

        var collection = new BoundingBoxCollection { bbox };

        Polygon polygon = collection.Polygon();

        polygon.Should().NotBeNull();
        polygon.IsValid.Should().BeTrue();
        polygon.Area.Should().BeGreaterThan(0);
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