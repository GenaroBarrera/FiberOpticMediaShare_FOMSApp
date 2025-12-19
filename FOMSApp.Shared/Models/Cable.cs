using NetTopologySuite.Geometries;

namespace FOMSApp.Shared.Models
{
    public class Cable
    {
        public int Id { get; set; } 
        
        public string Name { get; set; } = string.Empty;
        
        // e.g., "Orange", "Blue", "#FF5733"
        public string Color { get; set; } = "Orange"; // Default color is Orange
        
        // 'LineString' is the NTS type for a path/route
        public LineString? Path { get; set; }
    }
}