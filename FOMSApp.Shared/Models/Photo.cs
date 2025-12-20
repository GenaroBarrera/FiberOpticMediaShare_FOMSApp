using System;

namespace FOMSApp.Shared.Models
{
    /// <summary>
    /// Represents a construction photo uploaded for a specific vault.
    /// This is a "Child Entity" in a many-to-one relationship with Vault (many Photos belong to one Vault).
    /// </summary>
    /// <remarks>
    /// Best Practice: We store only the file name in the database, not the actual image bytes.
    /// This keeps the database small and fast. The actual file is stored in the wwwroot/uploads folder.
    /// This pattern is called "file reference" or "external storage".
    /// </remarks>
    public class Photo
    {
        /// <summary>
        /// Primary Key: Unique identifier for each photo record.
        /// Entity Framework automatically makes this an auto-incrementing identity column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The actual file name stored on the server (e.g., "dfd55694-62c8-4678-91f5-1c8aa7cc92bd.jpeg").
        /// 
        /// Best Practice: Files are renamed with GUIDs when uploaded to prevent naming conflicts
        /// and security issues. The original filename is not stored for simplicity.
        /// 
        /// To construct the full URL: {BaseUrl}/uploads/{FileName}
        /// Example: https://localhost:5001/uploads/myphoto.jpg
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of when the photo was uploaded to the system.
        /// Useful for auditing, sorting, and determining work completion times.
        /// 
        /// Note: Uses DateTime.Now (local server time). For distributed systems, consider DateTime.UtcNow
        /// and store timezone information separately.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Foreign Key: Links this photo to its parent Vault.
        /// This value must match an existing Vault.Id in the database.
        /// 
        /// Entity Framework uses this to:
        /// 1. Create a foreign key constraint in the database (ensures data integrity)
        /// 2. Enable queries like "get all photos for vault 5"
        /// 3. Support navigation properties (see Vault property below)
        /// 
        /// This is required - a photo must belong to a vault.
        /// </summary>
        public int VaultId { get; set; }

        /// <summary>
        /// Navigation Property: Reference back to the parent Vault object.
        /// 
        /// The '?' makes this nullable because:
        /// - When querying photos directly, you often don't need the full Vault object (saves memory/bandwidth)
        /// - Entity Framework only populates this if you use .Include(p => p.Vault) in your query
        /// 
        /// This allows you to navigate from a Photo to its Vault: photo.Vault.Name
        /// Example: var vaultName = photo.Vault?.Name; // Safe navigation with null-conditional operator
        /// </summary>
        public Vault? Vault { get; set; }
    }
}