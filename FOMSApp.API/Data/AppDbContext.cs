using Microsoft.EntityFrameworkCore;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Data;

// Database context for the FOMS application.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vault> Vaults { get; set; } = null!;
    public DbSet<Photo> Photos { get; set; } = null!;
    public DbSet<Cable> Cables { get; set; } = null!;
    public DbSet<Midpoint> Midpoints { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure spatial data type for SQL Server geography columns
        modelBuilder.Entity<Vault>()
            .Property(v => v.Location)
            .HasColumnType("geography");
        
        modelBuilder.Entity<Midpoint>()
            .Property(m => m.Location)
            .HasColumnType("geography");
        
        modelBuilder.Entity<Cable>()
            .Property(c => c.Path)
            .HasColumnType("geography");

        // Configure cascade delete for vault photos
        modelBuilder.Entity<Photo>()
            .HasOne(p => p.Vault)
            .WithMany(v => v.Photos)
            .HasForeignKey(p => p.VaultId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure cascade delete for midpoint photos
        modelBuilder.Entity<Photo>()
            .HasOne(p => p.Midpoint)
            .WithMany(m => m.Photos)
            .HasForeignKey(p => p.MidpointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
