using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models
{
    /// <summary>
    /// Represents a midpoint marker along a cable route (e.g., slack loops, splice points, equipment locations).
    /// Displayed as a small circle dot on the map, distinct from Vault pins.
    /// </summary>
    /// <remarks>
    /// Midpoints are typically used to mark intermediate points of interest along cable routes,
    /// such as where slack loops are stored, where splices occur, or where equipment is located.
    /// They provide visual reference points without the full functionality of a Vault.
    /// </remarks>
    public class Midpoint
    {
        /// <summary>
        /// Primary Key: Unique identifier for each midpoint marker.
        /// Entity Framework automatically makes this an auto-incrementing identity column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Human-readable name for the midpoint (e.g., "Slack Loop 1", "Splice Point A", "Equipment Box 5").
        /// Used for identification and display in popups on the map.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Color used to render the midpoint circle on the map.
        /// Can be a CSS color name (e.g., "Green", "Red", "Yellow") or hex code (e.g., "#00FF00").
        /// 
        /// Default is "Green" - commonly used for standard midpoints.
        /// Different colors can indicate different types of midpoints or their status.
        /// </summary>
        public string Color { get; set; } = "Green";

        /// <summary>
        /// Geographic coordinates (latitude/longitude) of the midpoint's location.
        /// Uses NetTopologySuite's Point type, which stores coordinates as (X=Longitude, Y=Latitude).
        /// 
        /// The '?' makes this nullable because:
        /// - A midpoint might be created before its exact location is determined
        /// - Allows for flexibility during the editing process
        /// 
        /// Entity Framework maps this to SQL Server's Geography type (configured in AppDbContext),
        /// enabling spatial queries and accurate distance calculations.
        /// </summary>
        public Point? Location { get; set; }
    }
}