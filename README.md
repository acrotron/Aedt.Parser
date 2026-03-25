# Aedt.Parser

A .NET library for parsing AEDT (Aviation Environmental Design Tool) CSV export files. Extracts bounding box polygons and receptor data as [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) geometries.

## Installation

```shell
dotnet add package Aedt.Parser
```

## Features

- **Bounding box parsing** - Read polygon coordinates from AEDT CSV exports
- **Bounding box collections** - Merge multiple bounding boxes into a single unified polygon
- **Self-intersection repair** - Automatically fixes invalid polygons (staircase patterns common in AEDT exports)
- **Receptor data parsing** - Read receptor points with longitude, latitude, and measured values

## Usage

### Reading a single polygon

```csharp
using Aedt.Parser;

var reader = new BoundingBoxReader();
BoundingBox boundingBox = reader.ReadPolygon("path/to/polygon.csv");
Polygon polygon = boundingBox.Polygon();
```

The CSV file should contain one coordinate pair (longitude, latitude) per line:

```
-73.87234,40.63456
-73.87235,40.63457
...
```

### Reading and merging multiple bounding boxes

```csharp
var reader = new BoundingBoxReader();
BoundingBoxCollection boxes = reader.ReadBoundingBoxes("path/to/boundingboxes.csv");
Polygon merged = boxes.Polygon();
```

Each line in the CSV contains all coordinate pairs for one bounding box (alternating longitude, latitude values):

```
-73.87,40.63,-73.88,40.63,-73.88,40.64,-73.87,40.64
-73.89,40.65,-73.90,40.65,-73.90,40.66,-73.89,40.66
```

Invalid polygons are automatically repaired and degenerate slivers are discarded during the merge.

### Reading receptor data

```csharp
var reader = new ReceptorsReader();
List<CoordinateM> receptors = reader.Read("path/to/receptors.csv");

foreach (CoordinateM receptor in receptors)
{
    double longitude = receptor.X;
    double latitude = receptor.Y;
    double value = receptor.M;
}
```

The CSV file should contain longitude, latitude, and value per line:

```
-73.87234,40.63456,55.3
-73.87235,40.63457,60.1
...
```

## Dependencies

- [NetTopologySuite](https://www.nuget.org/packages/NetTopologySuite) - Spatial geometry operations
- [ProjNET4GeoAPI](https://www.nuget.org/packages/ProjNET4GeoAPI) - Coordinate system transformations

## License

MIT
