using AwesomeAssertions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser.Tests;

[TestClass]
public class AedtParserTest
{
    [TestMethod]
    [DeploymentItem("DGNoiseValues_J6.csv")]
    public void ReceptorsReaderTest()
    {
        ReceptorsReader reader = new ReceptorsReader();
        List<CoordinateM> coordinates = reader.Read("DGNoiseValues_J6.csv");

        coordinates.Should().NotBeNull();
        coordinates.Count.Should().Be(1987);

        coordinates.All(x => !double.IsNaN(x.M)).Should().BeTrue();
        coordinates.Min(x => x.M).Should().Be(71.818603515625);
        coordinates.Max(x => x.M).Should().Be(129.076353016026);
    }

    [TestMethod]
    [DeploymentItem("PRBPolygon_J1010.csv")]
    public void BoundingBoxReaderPolygonTest()
    {
        BoundingBoxReader reader = new BoundingBoxReader();
        BoundingBox boundingBox = reader.ReadPolygon("PRBPolygon_J1010.csv");

        boundingBox.Should().NotBeNull();
        boundingBox.Coordinates.Count.Should().Be(1867);
        boundingBox.Coordinates[0].X.Should().Be(-111.62442);
        boundingBox.Coordinates[0].Y.Should().Be(36.78206);
    }

    [TestMethod]
    [DeploymentItem("DGBBoxes_J10.csv")]
    public void BoundingBoxReaderTest()
    {
        BoundingBoxReader reader = new BoundingBoxReader();
        BoundingBoxCollection boundingBoxes = reader.ReadBoundingBoxes("DGBBoxes_J10.csv");

        boundingBoxes.Should().NotBeNull();
        boundingBoxes.Count.Should().Be(1033);

        foreach (var box in boundingBoxes)
        {
            box.Coordinates.Count.Should().Be(4);
        }

        boundingBoxes[0].Coordinates[0].X.Should().Be(-122.51358);
        boundingBoxes[0].Coordinates[0].Y.Should().Be(37.87993);
    }
}