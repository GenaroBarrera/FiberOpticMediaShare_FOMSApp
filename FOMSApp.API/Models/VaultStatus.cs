namespace FOMSApp.API.Models
{
    // (The Workflow)
    // This is an Enumeration (Enum). It doesn't create a table; it creates a strict list of allowed "states" that a
    // vault can be in. It drives the color-coding on your map.
    public enum VaultStatus
    {
        // The default state. The vault exists on the map (Gray pin), but no work has been done yet.
        Pending = 0,    // Gray 

        // The field crew has uploaded photos. The pin turns Yellow, signaling the Coordinator to check it.
        Review = 1,     // Yellow 

        // The Coordinator approved the work. The pin turns Green, and the background sync to Google Drive begins.
        Complete = 2,   // Green 
        
        // The photos were bad. The pin turns Red, telling the crew to go back and fix it.
        Rejected = 3    // Red 
    }
}
