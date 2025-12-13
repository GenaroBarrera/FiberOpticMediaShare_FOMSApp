using NetTopologySuite.Geometries; //This is required to use the Point type for Location

namespace FOMSApp.API.Models
{
    // (The Parent Entity)
    // This class represents the physical object (vault) in the ground. It is the "Parent" because it owns the photos.
    public class Vault
    {
        // Id: The unique ID for the database (Primary Key).
        public int Id { get; set; } 

        // Name: The label found on the engineering prints (ex "Pg.1 Vault 1").
        public string Name { get; set; } = string.Empty; 
        
        // Native SQL Geography type
        // The most important field. It uses the Point type (from NetTopologySuite) to store the exact Latitude and Longitude. This is what allows us to plot it on a map.
        public Point Location { get; set; } 

        // Status: Stores the current state (from the list in VaultStatus).
        public VaultStatus Status { get; set; } = VaultStatus.Pending; 

        // Photos: A list (collection) of all photos attached to this specific vault. This creates a One-to-Many relationship (One Vault has Many Photos).
        public List<ConstructionPhoto> Photos { get; set; } = new(); 
    }
}
