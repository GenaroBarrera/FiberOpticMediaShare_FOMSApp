using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models;

// Represents a fiber optic vault (underground access point) in the network.
public class Vault
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Display name
    public string Color { get; set; } = "Blue"; // Map marker color
    public VaultStatus Status { get; set; } = VaultStatus.New; // Workflow status
    public string? Description { get; set; } // Optional notes
    public required Point Location { get; set; } // GPS coordinates (lon, lat)
    public List<Photo> Photos { get; set; } = new(); // Associated photos

    // Soft-delete flag. When true, the vault is hidden from normal lists/maps,
    // but remains in the database so it can be restored (Undo).
    public bool IsDeleted { get; set; } = false;

    // Timestamp for when the vault was soft-deleted (UTC). Used for retention/purge logic.
    public DateTimeOffset? DeletedAt { get; set; }
}
