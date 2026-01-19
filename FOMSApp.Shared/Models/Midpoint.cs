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

    // Soft-delete flag. When true, the midpoint is hidden from normal lists/maps,
    // but remains in the database so it can be restored (Undo).
    public bool IsDeleted { get; set; } = false;

    // Timestamp for when the midpoint was soft-deleted (UTC). Used for retention/purge logic.
    public DateTimeOffset? DeletedAt { get; set; }
}
