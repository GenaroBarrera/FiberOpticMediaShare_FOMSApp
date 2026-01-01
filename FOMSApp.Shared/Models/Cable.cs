using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models;

/// <summary>
/// Represents a fiber optic cable route displayed as a polyline on the map.
/// </summary>
public class Cable
{
    public int Id { get; set; }

    /// <summary>
    /// Display name for the cable route.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Polyline color (CSS color name or hex code).
    /// </summary>
    public string Color { get; set; } = "Black";

    /// <summary>
    /// Optional notes about the cable route.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Ordered sequence of coordinates forming the cable path.
    /// </summary>
    public LineString? Path { get; set; }
}
