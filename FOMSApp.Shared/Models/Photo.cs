namespace FOMSApp.Shared.Models;

// Represents a construction photo uploaded for a vault or midpoint.
public class Photo
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty; // Server-side filename (GUID-based)
    public DateTime UploadedAt { get; set; } = DateTime.Now; // Upload timestamp
    public int? VaultId { get; set; } // Parent vault ID (null if belongs to midpoint)
    public int? MidpointId { get; set; } // Parent midpoint ID (null if belongs to vault)
    public Vault? Vault { get; set; } // Navigation property
    public Midpoint? Midpoint { get; set; } // Navigation property
}
