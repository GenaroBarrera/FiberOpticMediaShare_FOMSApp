using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models;

/// <summary>
/// Represents a fiber optic vault (underground access point) in the network.
/// </summary>
public class Vault
{
    public int Id { get; set; }

    /// <summary>
    /// Display name for the vault.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Map marker color (CSS color name or hex code).
    /// </summary>
    public string Color { get; set; } = "Blue";

    /// <summary>
    /// Current workflow status. Determines marker color on the map.
    /// </summary>
    public VaultStatus Status { get; set; } = VaultStatus.New;

    /// <summary>
    /// Optional notes or observations about the vault.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// GPS coordinates (longitude, latitude) stored as Geography type.
    /// </summary>
    public required Point Location { get; set; }

    /// <summary>
    /// Photos associated with this vault.
    /// </summary>
    public List<Photo> Photos { get; set; } = new();
}
