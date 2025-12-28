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
    /// The numeric values (0, 1, 2, 3, 4) are stored in the database, but you work with the names in code.
    /// </summary>
    /// <remarks>
    /// Each status drives visual indicators on the map (pin colors) and determines
    /// business logic (e.g., only Complete vaults sync to Google Drive).
    /// 
    /// Status workflow progression:
    /// New (Blue) → Pending (Brown) → Review (Gray) → Complete (Green)
    /// Or: New → Issue (Red) if problems are found
    /// </remarks>
    public enum VaultStatus
    {
        /// <summary>
        /// Initial state when a new vault pin marker is placed on the map.
        /// Map display: Blue pin
        /// </summary>
        New = 0,

        /// <summary>
        /// Photos are pending for this vault. Waiting for field crew to upload photos.
        /// Map display: Brown pin
        /// </summary>
        Pending = 1,

        /// <summary>
        /// New photos have been uploaded and are waiting for coordinator review.
        /// Map display: Gray pin
        /// </summary>
        Review = 2,

        /// <summary>
        /// Photos are complete and approved. Ready for final processing or sync to Google Drive.
        /// Map display: Green pin
        /// </summary>
        Complete = 3,

        /// <summary>
        /// Issue present: missing photos or other problems that need to be addressed.
        /// Map display: Red pin
        /// </summary>
        Issue = 4
    }
}
