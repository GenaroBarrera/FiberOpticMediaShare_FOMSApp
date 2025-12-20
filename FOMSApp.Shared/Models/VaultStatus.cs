namespace FOMSApp.Shared.Models
{
    /// <summary>
    /// Enumeration defining the workflow states a vault can be in during the QA process.
    /// 
    /// Enums provide type-safe constants - they ensure only valid status values can be assigned.
    /// This is better than using strings because:
    /// - Compiler catches typos (can't assign "Pennding" by mistake)
    /// - IntelliSense shows available options
    /// - Database stores as integer (smaller, faster than strings)
    /// 
    /// This enum doesn't create a database table - Entity Framework stores it as an integer column.
    /// The numeric values (0, 1, 2, 3) are stored in the database, but you work with the names in code.
    /// </summary>
    /// <remarks>
    /// The workflow progression is typically:
    /// Pending → Review → Complete (approved) OR Rejected (needs rework)
    /// 
    /// Each status drives visual indicators on the map (pin colors) and determines
    /// business logic (e.g., only Complete vaults sync to Google Drive).
    /// </remarks>
    public enum VaultStatus
    {
        /// <summary>
        /// Initial state when a vault is created. No work has been done yet.
        /// Map display: Gray pin
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Field crew has uploaded photos for this vault. Waiting for coordinator review.
        /// Map display: Yellow pin
        /// </summary>
        Review = 1,

        /// <summary>
        /// Coordinator approved the work. Photos are ready to be synced to Google Drive.
        /// Map display: Green pin
        /// </summary>
        Complete = 2,

        /// <summary>
        /// Photos were rejected. Field crew needs to fix issues and re-upload.
        /// Map display: Red pin
        /// </summary>
        Rejected = 3
    }
}
