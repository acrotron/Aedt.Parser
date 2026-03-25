using NetTopologySuite.Geometries;

namespace Aedt.Parser;

public class BoundingBoxCollection : List<BoundingBox>
{
    /// <summary>
    /// Returns the merged bounding geometry as a single valid Polygon.
    /// Self-intersecting polygons (staircase patterns from AEDT CSV files) are
    /// fixed using Buffer(0), which splits them into valid parts. Degenerate
    /// slivers are discarded and the remaining parts are unioned into a single polygon.
    /// </summary>
    public Polygon Polygon()
    {
        List<Geometry> parts = new List<Geometry>();

        foreach (BoundingBox boundingBox in this)
        {
            Polygon polygon = boundingBox.Polygon();

            if (!polygon.IsValid)
            {
                // Buffer(0) fixes self-intersections by splitting into valid parts.
                // This may produce a MultiPolygon with degenerate slivers that we filter out.
                Geometry? repaired = polygon.Buffer(0);

                if (repaired is MultiPolygon multiPolygon)
                {
                    double maxArea = multiPolygon.Max(g => g.Area);
                    double threshold = maxArea * 1e-6;

                    foreach (Geometry? geometry in multiPolygon.Geometries)
                    {
                        if (geometry.Area > threshold)
                        {
                            parts.Add(geometry);
                        }
                    }                }
                else
                {
                    parts.Add(repaired);
                }
            }
            else
            {
                parts.Add(polygon);
            }
        }

        if (parts.Count == 0)
        {
            throw new InvalidOperationException("No valid bounding polygons.");
        }

        if (parts.Count == 1)
        {
            return (Polygon)parts[0];
        }

        Geometry? union = new GeometryCollection(parts.ToArray()).Union();

        return union switch
        {
            Polygon polygon => polygon,
            // If the union is a MultiPolygon, we take the largest part as the representative polygon.
            // we don't support separated bounding boxes, so we ignore smaller parts.
            MultiPolygon mp => (Polygon)mp.OrderByDescending(g => g.Area).First(),
            _ => throw new InvalidOperationException($"Unexpected geometry type: {union.GeometryType}")
        };
    }
}
