using NetTopologySuite.Geometries; // Required to use the Point type for geographic coordinates

namespace FOMSApp.Shared.Models
{
    /// <summary>
    /// Represents a physical fiber optic vault (underground access point) in the construction network.
    /// This is a "Parent Entity" in a one-to-many relationship with Photo entities.
    /// </summary>
    /// <remarks>
    /// Entity Framework Core will automatically create a database table called "Vaults" based on this class.
    /// The Location property uses SQL Server's Geography data type for spatial queries and mapping.
    /// </remarks>
    public class Vault
    {
        /// <summary>
        /// Primary Key: Unique identifier for each vault record in the database.
        /// Entity Framework automatically makes this an auto-incrementing identity column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Human-readable name for the vault (e.g., "Vault A1", "Downtown Vault", "North Route V-102").
        /// Using string.Empty as default prevents null reference exceptions when working with the property.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Color code for visual identification on the map. Can be a CSS color name or hex code.
        /// Default is "Blue" - this can be changed based on vault type or status if needed.
        /// </summary>
        public string Color { get; set; } = "Blue";

        /// <summary>
        /// Current workflow status of the vault (New, Pending, Review, Complete, Issue).
        /// This drives the visual indicator (color) on the map and determines if photos should be synced to Google Drive.
        /// Defaults to New (Blue) when a new vault pin marker is placed.
        /// </summary>
        public VaultStatus Status { get; set; } = VaultStatus.New;

        /// <summary>
        /// Optional description or comments about the vault.
        /// Used to store notes, observations, or additional context about the vault's condition or location.
        /// Can be edited by users on the vault details page.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Geographic coordinates (latitude/longitude) of the vault's physical location.
        /// Uses NetTopologySuite's Point type which stores coordinates as (X=Longitude, Y=Latitude).
        /// 
        /// The 'required' keyword ensures this property must be set when creating a Vault instance.
        /// This prevents runtime errors from null locations.
        /// 
        /// Entity Framework maps this to SQL Server's Geography type (configured in AppDbContext),
        /// which enables spatial queries like "find all vaults within 1km of a point".
        /// </summary>
        public required Point Location { get; set; }

        /// <summary>
        /// Navigation Property: Collection of all photos associated with this vault.
        /// This creates a one-to-many relationship in the database (one Vault can have many Photos).
        /// 
        /// Entity Framework uses this property to:
        /// 1. Create a foreign key (VaultId) in the Photos table
        /// 2. Enable eager loading with .Include(v => v.Photos)
        /// 3. Allow navigation from a Vault to its Photos: vault.Photos
        /// 
        /// Initialized as an empty list to prevent null reference exceptions.
        /// The list is automatically populated when you use .Include() in queries.
        /// </summary>
        public List<Photo> Photos { get; set; } = new();
    }
}
