using NetTopologySuite.Geometries; //This is required to use the Point type for Location

namespace FOMSApp.Shared.Models
{
    // (The Parent Entity)
    // This class represents the physical object (vault) in the ground. It is the "Parent" because it owns the photos.
    public class Vault
    {
        public int Id { get; set; } // Primary Key: Unique identifier for each vault
        public string Name { get; set; } = string.Empty; // Name of the vault (e.g., "Vault A1")
        public string Color { get; set; } = "Blue";// Type of vault (e.g., Standard, Custom, etc.)
        public VaultStatus Status { get; set; } = VaultStatus.Pending; // Status: Stores the current state (from the list in VaultStatus)

        // Native SQL Geography type
        // The most important field. It uses the Point type (from NetTopologySuite) to store the exact Latitude and Longitude. This is what allows us to plot it on a map.
        public required Point Location { get; set; } // Using 'required' to ensure this property is set during object initialization.
        
        // Photos: A list (collection) of all photos attached to this specific vault.
        public List<Photo> Photos { get; set; } = new();  //This creates a One-to-Many relationship (One Vault has Many Photos).
    }
}
