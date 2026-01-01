namespace FOMSApp.Shared.Models;

// Workflow states for vault QA process.
public enum VaultStatus
{
    New = 0,      // Blue marker
    Pending = 1,  // Brown marker
    Review = 2,   // Gray marker
    Complete = 3, // Green marker
    Issue = 4     // Red marker
}
