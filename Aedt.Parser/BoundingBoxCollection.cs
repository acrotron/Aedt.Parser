using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Aedt.Parser;

/// <summary>
/// The bounding boxes read from an AEDT bounding box file.
/// </summary>
public class BoundingBoxCollection : List<BoundingBox>
{
    /// <summary>
    /// Returns the merged bounding geometry as a single valid Polygon.
    /// Invalid boxes (e.g. self-intersecting staircase patterns from AEDT CSV files) are repaired with
    /// <see cref="GeometryFixer"/>, which keeps the full area of every lobe. Degenerate boxes and slivers are
    /// discarded, and the remaining parts are unioned. When the union falls apart into separate polygons, only the
    /// largest one is returned.
    /// </summary>
    /// <exception cref="InvalidOperationException">No box has a non-zero area.</exception>
    public Polygon Polygon()
    {
        List<Geometry> parts = new List<Geometry>();

        foreach (BoundingBox boundingBox in this)
        {
            Polygon polygon = boundingBox.Polygon();

            if (polygon.IsValid)
            {
                parts.Add(polygon);
                continue;
            }

            Geometry repaired = GeometryFixer.Fix(polygon);

            if (repaired is MultiPolygon multiPolygon)
            {
                // Drop degenerate slivers left over from the repair.
                double threshold = multiPolygon.Max(g => g.Area) * 1e-6;

                parts.AddRange(multiPolygon.Geometries.Where(g => g.Area > threshold));
            }
            else if (repaired is Polygon { IsEmpty: false } repairedPolygon)
            {
                parts.Add(repairedPolygon);
            }

            // Anything else is a box that collapsed to nothing (e.g. collinear points).
        }

        if (parts.Count == 0)
        {
            throw new InvalidOperationException("No valid bounding polygons.");
        }

        if (parts.Count == 1)
        {
            return (Polygon)parts[0];
        }

        Geometry union = new GeometryCollection(parts.ToArray()).Union();

        return union switch
        {
            Polygon polygon => polygon,
            // Separate bounding areas are not supported, so only the largest part is kept.
            MultiPolygon mp => (Polygon)mp.OrderByDescending(g => g.Area).First(),
            _ => throw new InvalidOperationException($"Unexpected geometry type: {union.GeometryType}")
        };
    }
}
