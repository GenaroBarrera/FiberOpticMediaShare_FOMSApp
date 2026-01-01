using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models;

// Represents a midpoint marker along a cable route (slack loops, splice points, etc.).
public class Midpoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Display name
    public string Color { get; set; } = "Black"; // Map marker color
    public MidpointStatus Status { get; set; } = MidpointStatus.New; // Workflow status
    public string? Description { get; set; } // Optional notes
    public Point? Location { get; set; } // GPS coordinates (lon, lat)
    public List<Photo> Photos { get; set; } = new(); // Associated photos
}
