using Microsoft.EntityFrameworkCore; // EF Core namespace for DbContext and related classes
using FOMSApp.API.Models; // Namespace where Vault and ConstructionPhoto classes are defined

namespace FOMSApp.API.Data
{
    // Think of AppDbContext.cs as a translator between your C# application and the SQL database. 
    public class AppDbContext : DbContext // Inherits from Entity Framework's DbContext
    {
        // Constructor: Passes configuration options (like connection strings) to the base class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // Constructor

        // These properties become your Database Tables
        public DbSet<Vault> Vaults { get; set; } = null!; // The Vaults table. cannot be null
        public DbSet<ConstructionPhoto> ConstructionPhotos { get; set; } = null!; // The ConstructionPhotos table. cannot be null

        // OnModelCreating: In this method we tell the sql server how to treat our spatial data
        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);

            // CRITICAL: specific configuration for Spatial Data
            // This tells SQL Server: "Treat Location as a Geography (Earth coordinates), not Geometry (Flat map)"
            modelBuilder.Entity<Vault>()
                .Property(v => v.Location)
                .HasColumnType("geography");
        }
    }
}

/* Think of AppDbContext.cs as a Translator.
Your application speaks C# (Objects, Classes, Lists). Your database speaks SQL (Tables, Rows, Columns).
The AppDbContext (which inherits from DbContext in Entity Framework) sits in the middle and translates between the two
languages automatically. */