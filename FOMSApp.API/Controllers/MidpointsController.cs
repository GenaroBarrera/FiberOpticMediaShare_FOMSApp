using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    /// <summary>
    /// RESTful API controller for managing Midpoint entities.
    /// Handles HTTP requests for creating and reading midpoint markers on the map.
    /// </summary>
    /// <remarks>
    /// Endpoints:
    /// - GET /api/midpoints - Get all midpoints
    /// - POST /api/midpoints - Create a new midpoint
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class MidpointsController : ControllerBase
    {
        /// <summary>
        /// Database context for accessing midpoint data.
        /// Injected via constructor dependency injection.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor that receives the database context via dependency injection.
        /// </summary>
        /// <param name="context">The database context instance</param>
        public MidpointsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all midpoints from the database.
        /// </summary>
        /// <returns>
        /// HTTP 200 OK with a list of all midpoints.
        /// </returns>
        /// <remarks>
        /// Unlike Vaults, midpoints don't have related entities to include, so we use a simple query.
        /// This is efficient for displaying all midpoints on the map simultaneously.
        /// </remarks>
        // GET: api/midpoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Midpoint>>> GetMidpoints()
        {
            // Simple query: get all midpoints and return as a list
            // ToListAsync() executes the query asynchronously
            return await _context.Midpoints.ToListAsync();
        }

        /// <summary>
        /// Creates a new midpoint in the database.
        /// </summary>
        /// <param name="midpoint">
        /// The midpoint object to create. Must include Name, Color, and Location properties.
        /// </param>
        /// <returns>
        /// HTTP 201 Created with the newly created midpoint (including its auto-generated ID).
        /// Returns HTTP 400 Bad Request if model validation fails.
        /// </returns>
        /// <remarks>
        /// Best Practice: Following REST conventions by returning 201 Created status
        /// with a Location header pointing to the new resource.
        /// </remarks>
        // POST: api/midpoints
        [HttpPost]
        public async Task<ActionResult<Midpoint>> PostMidpoint(Midpoint midpoint)
        {
            // Add to change tracker (marks entity for insertion)
            _context.Midpoints.Add(midpoint);
            
            // Execute the SQL INSERT statement
            await _context.SaveChangesAsync();
            
            // Return 201 Created with Location header
            return CreatedAtAction(nameof(GetMidpoints), new { id = midpoint.Id }, midpoint);
        }

        /// <summary>
        /// Deletes a midpoint from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the midpoint to delete (from the URL route)</param>
        /// <returns>
        /// HTTP 204 No Content if successfully deleted, or HTTP 404 Not Found if the midpoint doesn't exist.
        /// </returns>
        /// <remarks>
        /// RESTful Best Practice: DELETE operations typically return 204 No Content (success with no body)
        /// rather than 200 OK, because there's nothing meaningful to return after deletion.
        /// </remarks>
        // DELETE: api/midpoints/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMidpoint(int id)
        {
            // Find the midpoint by primary key
            var midpoint = await _context.Midpoints.FindAsync(id);
            
            // Return 404 if not found (standard REST practice)
            if (midpoint == null)
                return NotFound();

            // Mark the entity for deletion
            _context.Midpoints.Remove(midpoint);
            
            // Execute the SQL DELETE statement
            await _context.SaveChangesAsync();
            
            // Return 204 No Content (successful deletion with no response body)
            return NoContent();
        }
    }
}