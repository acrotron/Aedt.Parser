using NetTopologySuite.Geometries;
using System.Globalization;

namespace Aedt.Parser;

public sealed class ReceptorsReader
{
    public List<CoordinateM> Read(string filePath)
    {
        var points = new List<CoordinateM>();
        using var reader = new StreamReader(filePath);

        int row = 0;

        while (reader.ReadLine() is { } line)
        {
            // Parse each line of the .grd file
            // Assuming each line contains values corresponding to a grid row
            string[] values = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (values.Length > 2)
            {
                // Convert value to double, assuming it represents the associated value at this grid point
                double value = double.Parse(values[2], CultureInfo.InvariantCulture);

                // CSV format: longitude, latitude, value
                // Round to 6 decimal places to match Python: float(format(float(x), '.6f'))
                double longitude = Math.Round(double.Parse(values[0], NumberFormatInfo.InvariantInfo), 6);
                double latitude = Math.Round(double.Parse(values[1], NumberFormatInfo.InvariantInfo), 6);

                // CoordinateM: X=longitude, Y=latitude (geographic convention)
                var point = new CoordinateM(longitude, latitude, value);

                // You can associate the value with the point here (e.g., store in a dictionary, or attach attributes)
                // For now, we just add the point to the list
                points.Add(point);
            }

            row++;
        }

        return points;
    }
}