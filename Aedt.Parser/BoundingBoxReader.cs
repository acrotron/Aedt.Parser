using System.Globalization;
using NetTopologySuite.Geometries;

namespace Aedt.Parser;

public sealed class BoundingBoxReader
{
    public BoundingBox ReadPolygon(string filePath)
    {
        BoundingBox boundingBox = new BoundingBox();
        using StreamReader reader = new StreamReader(filePath);

        while (reader.ReadLine() is { } line)
        {
            // Parse each line of the .grd file
            // Assuming each line contains values corresponding to a grid row
            string[] values = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (values.Length > 1)
            {
                // Convert value to double, assuming it represents the associated value at this grid point
                string @long = values[0];
                string lat = values[1];

                // Assuming a specific grid resolution, we can calculate lat/lon
                // For simplicity, assuming fixed lat/lon increments (e.g., 1 degree per row/col)
                // Modify the following as needed based on your .grd file specifications
                double longitude = Math.Round(double.Parse(@long, NumberFormatInfo.InvariantInfo), 5);
                double latitude = Math.Round(double.Parse(lat, NumberFormatInfo.InvariantInfo), 5);

                // Create a coordinate with NetTopologySuite
                Coordinate point = new Coordinate(longitude, latitude);

                // You can associate the value with the point here (e.g., store in a dictionary, or attach attributes)
                // For now, we just add the point to the list
                boundingBox.Coordinates.Add(point);
            }
        }

        return boundingBox;
    }

    public BoundingBoxCollection ReadBoundingBoxes(string filePath)
    {
        BoundingBoxCollection points = new BoundingBoxCollection();
        using StreamReader reader = new StreamReader(filePath);

        while (reader.ReadLine() is { } line)
        {
            BoundingBox boundingBox = new BoundingBox();

            // Parse each line of the .grd file
            // Assuming each line contains values corresponding to a grid row
            string[] values = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (values.Length > 0 && values.Length % 2 == 0)
            {
                for (int i = 0; i < values.Length; i += 2)
                {
                    string @long = values[i];
                    string lat = values[i+1];

                    // Assuming a specific grid resolution, we can calculate lat/lon
                    // For simplicity, assuming fixed lat/lon increments (e.g., 1 degree per row/col)
                    // Modify the following as needed based on your .grd file specifications
                    double longitude = Math.Round(double.Parse(@long, NumberFormatInfo.InvariantInfo), 5);
                    double latitude = Math.Round(double.Parse(lat, NumberFormatInfo.InvariantInfo), 5);

                    // Create a coordinate with NetTopologySuite
                    boundingBox.Coordinates.Add(new Coordinate(longitude, latitude));
                }

                // You can associate the value with the point here (e.g., store in a dictionary, or attach attributes)
                // For now, we just add the point to the list
                points.Add(boundingBox);
            }
        }

        return points;
    }
}