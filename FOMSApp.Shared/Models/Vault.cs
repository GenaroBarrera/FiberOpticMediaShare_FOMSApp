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
}
