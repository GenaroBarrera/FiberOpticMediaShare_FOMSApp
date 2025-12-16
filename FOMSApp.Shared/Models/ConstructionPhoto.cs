namespace FOMSApp.Shared.Models
{
    // (The Child Entity)
    // This represents a single image uploaded by a field crew member.
    public class ConstructionPhoto 
    {
        // Id: The unique ID for this specific photo record.
        public int Id { get; set; }

        // Url: Performance Key. We do not store the image file inside the database (that would be slow). We upload the
        // file to Azure Storage, get a web link (URL), and store just that text string here.
        public string Url { get; set; } = string.Empty; 

        // PhotoType: Describes what the photo is showing (e.g., "Closed Lid", "Conduit Entry", "Label").
        public string PhotoType { get; set; } = string.Empty; 

        // UploadedAt: A timestamp so you know exactly when the work was done.
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // VaultId: The Foreign Key. It creates the link back to the specific Vault this photo belongs to.
        public int VaultId { get; set; }

        // Vault: The Navigation Property. It allows easy access to the parent Vault object from this photo.
        public Vault? Vault { get; set; } //refers to the parent Vault object. ? means it can be null.
    }
}
// Relationship summary: One Vault can have many ConstructionPhotos. 
// When you run the migration, Entity Framework reads these files and builds two tables (Vaults and
// ConstructionPhotos) linked together by that VaultId.