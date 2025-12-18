using System;

namespace FOMSApp.Shared.Models
{
    public class Photo
    {
        // Primary Key
        public int Id { get; set; } // Unique identifier for each photo
        
        // The actual file name stored on the server (e.g., "vault1_photo.jpg")
        public string FileName { get; set; } = string.Empty; 
        
        // When was it taken?
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Foreign Key: Which Vault does this belong to?
        public int VaultId { get; set; } // Links to the Vault's Id
        
        // Navigation Property: Link back to the Vault object (optional but useful)
        // We use '?' because sometimes we just want the photo data without loading the whole vault
        public Vault? Vault { get; set; }
    }
}