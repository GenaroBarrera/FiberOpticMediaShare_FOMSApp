using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models;

/// <summary>
/// Represents a midpoint marker along a cable route (slack loops, splice points, etc.).
/// </summary>
public class Midpoint
{
    public int Id { get; set; }

    /// <summary>
    /// Display name for the midpoint.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Map marker color (CSS color name or hex code).
    /// </summary>
    public string Color { get; set; } = "Black";

    /// <summary>
    /// Current workflow status. Determines marker color on the map.
    /// </summary>
    public MidpointStatus Status { get; set; } = MidpointStatus.New;

    /// <summary>
    /// Optional notes or observations about the midpoint.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// GPS coordinates (longitude, latitude) stored as Geography type.
    /// </summary>
    public Point? Location { get; set; }

    /// <summary>
    /// Photos associated with this midpoint.
    /// </summary>
    public List<Photo> Photos { get; set; } = new();
}
