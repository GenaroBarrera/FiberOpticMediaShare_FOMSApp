using NetTopologySuite.Geometries; 

namespace FOMSApp.API.Models
{
    public class Vault
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        
        // Native SQL Geography type
        public Point Location { get; set; } 

        public VaultStatus Status { get; set; } = VaultStatus.Pending;

        public List<ConstructionPhoto> Photos { get; set; } = new();
    }
}
