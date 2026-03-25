using AwesomeAssertions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser.Tests;

[TestClass]
public class BoundingBoxReaderTests
{

    [TestMethod]
    [DeploymentItem("PRBPolygon_J1010.csv")]
    public void Polygon_FromFile_ReturnsValidPolygon()
    {
        var reader = new BoundingBoxReader();
        BoundingBox bbox = reader.ReadPolygon("PRBPolygon_J1010.csv");

        Polygon polygon = bbox.Polygon();

        polygon.Should().NotBeNull();
        polygon.ExteriorRing.IsClosed.Should().BeTrue();
        polygon.Area.Should().BeGreaterThan(0);
    }
}