using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models
{
    public class Midpoint
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty; // e.g. "Slack Loop 1"
        
        // We can use distinct colors (e.g. "Green", "Red")
        public string Color { get; set; } = "Green"; 

        public Point? Location { get; set; }
    }
}