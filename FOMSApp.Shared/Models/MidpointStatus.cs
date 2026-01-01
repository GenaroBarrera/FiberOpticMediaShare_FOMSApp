namespace FOMSApp.Shared.Models;

// Workflow states for midpoint QA process.
public enum MidpointStatus
{
    New = 0,      // Black marker
    Review = 1,   // Light gray marker
    Complete = 2, // Light green marker
    Issue = 3     // Light red marker
}
