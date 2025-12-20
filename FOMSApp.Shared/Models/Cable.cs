using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models
{
    /// <summary>
    /// Represents a fiber optic cable route displayed as a polyline on the map.
    /// A cable is defined by a series of connected points forming a path (LineString).
    /// </summary>
    /// <remarks>
    /// Unlike Vault and Midpoint which are single points, Cable represents a linear feature
    /// that connects multiple locations. Think of it as the conduit that runs between vaults.
    /// </remarks>
    public class Cable
    {
        /// <summary>
        /// Primary Key: Unique identifier for each cable route.
        /// Entity Framework automatically makes this an auto-incrementing identity column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Human-readable name for the cable route (e.g., "Main Backbone", "North Loop Cable", "Fiber Route A").
        /// Used for identification and display in popups on the map.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Color used to render the cable polyline on the map.
        /// Can be a CSS color name (e.g., "Orange", "Blue", "Red") or hex code (e.g., "#FF5733").
        /// 
        /// Default is "Orange" - a common color for fiber optic cables in engineering diagrams.
        /// Different colors can represent different cable types, capacities, or installation phases.
        /// </summary>
        public string Color { get; set; } = "Orange";

        /// <summary>
        /// The geographic path of the cable as a series of connected points.
        /// Uses NetTopologySuite's LineString type, which stores an ordered sequence of coordinates.
        /// 
        /// Coordinates are stored as (X=Longitude, Y=Latitude) pairs.
        /// Example: A cable from Austin to Dallas would have points: (-97.7431, 30.2672) -> (-96.7970, 32.7767)
        /// 
        /// The '?' makes this nullable because:
        /// - A cable might be created before its path is fully defined
        /// - Allows for flexibility during the drawing/editing process
        /// 
        /// Entity Framework maps this to SQL Server's Geography type (similar to Point),
        /// enabling spatial queries like "find all cables that pass through this area".
        /// </summary>
        public LineString? Path { get; set; }
    }
}