using NetTopologySuite;
using NetTopologySuite.Geometries;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Data
{
    /// <summary>
    /// Database initializer for seeding initial data.
    /// Currently disabled - no seeded data is created.
    /// All data should be created through the application UI.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initializes the database. Currently does nothing - no seeded data is created.
        /// All vaults, midpoints, and cables should be created through the application UI.
        /// </summary>
        /// <param name="context">The database context</param>
        public static void Initialize(AppDbContext context)
        {
            // Seeded data has been removed - all data should be created through the application UI
            // This method is kept for potential future use but currently does not seed any data
            return;
        }
    }
}