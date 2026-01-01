namespace FOMSApp.Shared.Models;

/// <summary>
/// Workflow states for midpoint QA process.
/// </summary>
public enum MidpointStatus
{
    /// <summary>New - Black marker</summary>
    New = 0,

    /// <summary>Awaiting review - Light gray marker</summary>
    Review = 1,

    /// <summary>Approved - Light green marker</summary>
    Complete = 2,

    /// <summary>Issue found - Light red marker</summary>
    Issue = 3
}
