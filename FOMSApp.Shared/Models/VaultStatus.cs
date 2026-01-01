namespace FOMSApp.Shared.Models;

/// <summary>
/// Workflow states for vault QA process.
/// </summary>
public enum VaultStatus
{
    /// <summary>New - Blue marker</summary>
    New = 0,

    /// <summary>Pending photos - Brown marker</summary>
    Pending = 1,

    /// <summary>Awaiting review - Gray marker</summary>
    Review = 2,

    /// <summary>Approved - Green marker</summary>
    Complete = 3,

    /// <summary>Issue found - Red marker</summary>
    Issue = 4
}
