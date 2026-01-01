namespace FOMSApp.Shared.Models;

/// <summary>
/// Represents a construction photo uploaded for a vault or midpoint.
/// </summary>
public class Photo
{
    public int Id { get; set; }

    /// <summary>
    /// Server-side filename (GUID-based to prevent conflicts).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Upload timestamp.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Parent vault ID (null if belongs to midpoint).
    /// </summary>
    public int? VaultId { get; set; }

    /// <summary>
    /// Parent midpoint ID (null if belongs to vault).
    /// </summary>
    public int? MidpointId { get; set; }

    /// <summary>
    /// Navigation property to parent vault.
    /// </summary>
    public Vault? Vault { get; set; }

    /// <summary>
    /// Navigation property to parent midpoint.
    /// </summary>
    public Midpoint? Midpoint { get; set; }
}
