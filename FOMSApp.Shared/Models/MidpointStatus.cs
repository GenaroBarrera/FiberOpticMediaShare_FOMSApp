namespace FOMSApp.Shared.Models
{
    /// <summary>
    /// Enumeration defining the workflow states a midpoint can be in during the QA process.
    /// 
    /// Enums provide type-safe constants - they ensure only valid status values can be assigned.
    /// This is better than using strings because:
    /// - Compiler catches typos (can't assign invalid values by mistake)
    /// - IntelliSense shows available options
    /// - Database stores as integer (smaller, faster than strings)
    /// 
    /// This enum doesn't create a database table - Entity Framework stores it as an integer column.
    /// The numeric values (0, 1, 2, 3) are stored in the database, but you work with the names in code.
    /// </summary>
    /// <remarks>
    /// Each status drives visual indicators on the map (marker colors) and determines
    /// business logic for midpoint workflow management.
    /// 
    /// Status workflow progression:
    /// New (Black) → Review (Light Gray) → Complete (Light Green)
    /// Or: New → Issue (Light Red) if problems are found
    /// </remarks>
    public enum MidpointStatus
    {
        /// <summary>
        /// Initial state when a new midpoint marker is placed on the map.
        /// Map display: Black marker
        /// </summary>
        New = 0,

        /// <summary>
        /// New photos have been uploaded and are waiting for coordinator review.
        /// Map display: Light Gray marker
        /// </summary>
        Review = 1,

        /// <summary>
        /// Photos are complete and approved. Ready for final processing.
        /// Map display: Light Green marker
        /// </summary>
        Complete = 2,

        /// <summary>
        /// Issue present: missing photos or other problems that need to be addressed
        /// (e.g., trench not dug deep enough).
        /// Map display: Light Red marker
        /// </summary>
        Issue = 3
    }
}
