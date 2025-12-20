using Microsoft.EntityFrameworkCore;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Data
{
    /// <summary>
    /// Database context class that acts as a bridge between your C# code and the SQL Server database.
    /// 
    /// Think of it as a translator: your application speaks C# (objects, classes, lists),
    /// and your database speaks SQL (tables, rows, columns). AppDbContext sits in the middle
    /// and automatically translates between the two languages.
    /// 
    /// This class inherits from Entity Framework Core's DbContext, which provides:
    /// - Change tracking (knows which entities were added/modified/deleted)
    /// - Query translation (converts LINQ to SQL)
    /// - Relationship management (handles foreign keys and navigation properties)
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Constructor that receives database configuration options.
        /// 
        /// The options typically include:
        /// - Connection string (which database to connect to)
        /// - Database provider (SQL Server, PostgreSQL, etc.)
        /// - Other settings (command timeout, logging, etc.)
        /// 
        /// These options are configured in Program.cs when the DbContext is registered.
        /// </summary>
        /// <param name="options">Configuration options passed from dependency injection</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// DbSet represents a table in the database.
        /// Each DbSet<T> property becomes a SQL table named after the entity (usually pluralized).
        /// 
        /// The '= null!' means "this will never be null, trust me" - it tells the compiler
        /// to suppress null warnings. Entity Framework Core initializes these properties
        /// automatically when the context is created.
        /// 
        /// Usage examples:
        /// - _context.Vaults.ToList() - SELECT * FROM Vaults
        /// - _context.Vaults.Add(vault) - INSERT INTO Vaults ...
        /// - _context.Vaults.Find(5) - SELECT * FROM Vaults WHERE Id = 5
        /// </summary>
        public DbSet<Vault> Vaults { get; set; } = null!;

        /// <summary>
        /// Photos table - stores metadata about uploaded construction photos.
        /// </summary>
        public DbSet<Photo> Photos { get; set; } = null!;

        /// <summary>
        /// Cables table - stores cable routes as LineString geometries.
        /// </summary>
        public DbSet<Cable> Cables { get; set; } = null!;

        /// <summary>
        /// Midpoints table - stores midpoint markers as Point geometries.
        /// </summary>
        public DbSet<Midpoint> Midpoints { get; set; } = null!;

        /// <summary>
        /// This method is called when Entity Framework Core is building the database model.
        /// Use it to configure how your entities map to database tables/columns.
        /// 
        /// This is where you can:
        /// - Configure column types (like we do for spatial data)
        /// - Set up indexes for performance
        /// - Define relationships between tables
        /// - Configure default values, constraints, etc.
        /// </summary>
        /// <param name="modelBuilder">
        /// The model builder provides a fluent API for configuring entity properties.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call the base implementation first to apply default configurations
            base.OnModelCreating(modelBuilder);

            /// CRITICAL: Configure spatial data type for Vault locations.
            /// 
            /// SQL Server has two spatial data types:
            /// - Geography: Uses Earth's curved surface (WGS 84 coordinate system)
            ///              Best for GPS coordinates, real-world distances, mapping
            /// - Geometry: Uses flat plane (assumes flat Earth)
            ///             Best for floor plans, abstract coordinates
            /// 
            /// Since we're working with real-world GPS coordinates (latitude/longitude),
            /// we MUST use Geography. This ensures:
            /// - Accurate distance calculations (accounts for Earth's curvature)
            /// - Proper mapping in Leaflet.js
            /// - Correct spatial queries (e.g., "find all vaults within 1km")
            /// 
            /// Without this configuration, Entity Framework might default to Geometry,
            /// which would give incorrect results for geographic data.
            modelBuilder.Entity<Vault>()
                .Property(v => v.Location) // Configure the Location property
                .HasColumnType("geography"); // Store as SQL Server Geography type

            // Note: Midpoint and Cable also have spatial properties (Point, LineString),
            // but Entity Framework Core with NetTopologySuite typically infers the correct
            // type automatically. If you encounter issues, you can configure them here too.
        }
    }
}