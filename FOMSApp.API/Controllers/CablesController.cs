using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    /// <summary>
    /// RESTful API controller for managing Cable entities.
    /// Handles HTTP requests for creating, reading, and deleting cable routes on the map.
    /// </summary>
    /// <remarks>
    /// Endpoints:
    /// - GET /api/cables - Get all cable routes
    /// - POST /api/cables - Create a new cable route
    /// - DELETE /api/cables/{id} - Delete a cable route
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class CablesController : ControllerBase
    {
        /// <summary>
        /// Database context for accessing cable data.
        /// Injected via constructor dependency injection.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor that receives the database context via dependency injection.
        /// </summary>
        /// <param name="context">The database context instance</param>
        public CablesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all cable routes from the database.
        /// </summary>
        /// <returns>
        /// HTTP 200 OK with a list of all cables, each containing its Path (LineString) with coordinates.
        /// </returns>
        /// <remarks>
        /// Used by the frontend to render all cable polylines on the map.
        /// The Path property contains the LineString geometry with all coordinate points.
        /// </remarks>
        // GET: api/cables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cable>>> GetCables()
        {
            // Retrieve all cables - Entity Framework automatically serializes the LineString geometry
            return await _context.Cables.ToListAsync();
        }

        /// <summary>
        /// Creates a new cable route in the database.
        /// </summary>
        /// <param name="cable">
        /// The cable object to create. Must include Name, Color, and Path (LineString) properties.
        /// The Path should contain at least 2 points to form a valid line.
        /// </param>
        /// <returns>
        /// HTTP 201 Created with the newly created cable (including its auto-generated ID).
        /// Returns HTTP 400 Bad Request if model validation fails.
        /// </returns>
        /// <remarks>
        /// The Path LineString is typically created by clicking multiple points on the map
        /// in the frontend, then sent to this endpoint as a complete geometry.
        /// </remarks>
        // POST: api/cables
        [HttpPost]
        public async Task<ActionResult<Cable>> PostCable(Cable cable)
        {
            // Add to change tracker (marks entity for insertion)
            _context.Cables.Add(cable);
            
            // Execute the SQL INSERT statement (includes the LineString geometry)
            await _context.SaveChangesAsync();
            
            // Return 201 Created with Location header
            return CreatedAtAction(nameof(GetCables), new { id = cable.Id }, cable);
        }

        /// <summary>
        /// Deletes a cable route from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the cable to delete (from the URL route)</param>
        /// <returns>
        /// HTTP 204 No Content if successfully deleted, or HTTP 404 Not Found if the cable doesn't exist.
        /// </returns>
        /// <remarks>
        /// RESTful Best Practice: DELETE operations typically return 204 No Content (success with no body)
        /// rather than 200 OK, because there's nothing meaningful to return after deletion.
        /// 
        /// Security Consideration: In a production app, you might want to add authorization checks
        /// to ensure only authorized users can delete cables.
        /// </remarks>
        // DELETE: api/cables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCable(int id)
        {
            // Find the cable by primary key
            var cable = await _context.Cables.FindAsync(id);
            
            // Return 404 if not found (standard REST practice)
            if (cable == null) 
                return NotFound();

            // Mark the entity for deletion
            _context.Cables.Remove(cable);
            
            // Execute the SQL DELETE statement
            await _context.SaveChangesAsync();
            
            // Return 204 No Content (successful deletion with no response body)
            return NoContent();
        }
    }
}