using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models;

// Represents a fiber optic cable route displayed as a polyline on the map.
public class Cable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Display name
    public string Color { get; set; } = "Black"; // Polyline color
    public string? Description { get; set; } // Optional notes
    public LineString? Path { get; set; } // Ordered coordinates forming the cable path
}
