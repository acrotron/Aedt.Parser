using Aedt.Parser.Exceptions;
using AwesomeAssertions;
using NetTopologySuite.Geometries;

namespace Aedt.Parser.Tests;

[TestClass]
public class CsvParsingTests
{
    private readonly List<string> _files = [];

    [TestCleanup]
    public void Cleanup()
    {
        foreach (string file in _files)
        {
            File.Delete(file);
        }
    }

    [TestMethod]
    public void ReceptorsReader_EndTrailerAndBlankLines_AreSkipped()
    {
        // Arrange
        string file = WriteFile("-122.1, 37.6, 55.5", "", "-122.2,37.7,60", "END,,");

        // Act
        List<CoordinateM> points = new ReceptorsReader().Read(file);

        // Assert
        points.Should().HaveCount(2);
        points[1].X.Should().Be(-122.2);
        points[1].Y.Should().Be(37.7);
        points[1].M.Should().Be(60);
    }

    [TestMethod]
    [DataRow("-122.1,,55.5,1")] // empty latitude used to shift 55.5 into the latitude column
    [DataRow("-122.1,37.6,")]
    [DataRow("-122.1,37.6")]
    [DataRow("-122.1,37.6, ")]
    [DataRow("Longitude,Latitude,Value")]
    public void ReceptorsReader_MalformedLine_ThrowsWithLineNumber(string malformed)
    {
        // Arrange
        string file = WriteFile("-122.0,37.5,50", malformed, "END,,");

        // Act
        Action act = () => new ReceptorsReader().Read(file);

        // Assert
        act.Should().Throw<AedtFormatException>().Which.LineNumber.Should().Be(2);
    }

    [TestMethod]
    [DataRow(-11.8761555, -11.876155)] // Math.Round(x, 6) gives -11.876156 here
    [DataRow(-122.3853314, -122.385331)]
    [DataRow(37.6106585, 37.610658)] // binary value is just below the midpoint
    public void ReceptorsReader_Coordinates_RoundLikePythonFormat(double input, double expected)
    {
        // Arrange - expected values are Python's float(format(input, '.6f'))
        string file = WriteFile($"{input.ToString("R", System.Globalization.CultureInfo.InvariantCulture)},0,1");

        // Act
        List<CoordinateM> points = new ReceptorsReader().Read(file);

        // Assert
        points[0].X.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("-122.1,,-122.2,37.7,-122.2,37.8,-122.1,37.8")]
    [DataRow("-122.1,37.6,-122.2,37.7,-122.2")]
    [DataRow("-122.1,37.6,-122.2,37.7,-122.2,37.8,-122.1,x")]
    public void BoundingBoxReader_MalformedBox_ThrowsWithLineNumber(string malformed)
    {
        // Arrange
        string file = WriteFile("-122.1, 37.6, -122.1, 37.7, -122.0, 37.7, -122.0, 37.6", malformed, "END,,");

        // Act
        Action act = () => new BoundingBoxReader().ReadBoundingBoxes(file);

        // Assert
        act.Should().Throw<AedtFormatException>().Which.LineNumber.Should().Be(2);
    }

    [TestMethod]
    [DataRow("-111.6,")]
    [DataRow("-111.6")]
    [DataRow("-111.6,36.7,1")]
    public void BoundingBoxReader_MalformedPolygonVertex_ThrowsWithLineNumber(string malformed)
    {
        // Arrange
        string file = WriteFile("-111.62,36.78", malformed, "END");

        // Act
        Action act = () => new BoundingBoxReader().ReadPolygon(file);

        // Assert
        act.Should().Throw<AedtFormatException>().Which.LineNumber.Should().Be(2);
    }

    private string WriteFile(params string[] lines)
    {
        string file = Path.GetTempFileName();
        _files.Add(file);
        File.WriteAllLines(file, lines);
        return file;
    }
}
