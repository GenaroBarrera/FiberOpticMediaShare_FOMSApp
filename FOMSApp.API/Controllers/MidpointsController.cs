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
        /// Retrieves a single midpoint by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the midpoint to retrieve (from the URL route)</param>
        /// <returns>
        /// HTTP 200 OK with the midpoint if found, or HTTP 404 Not Found if the midpoint doesn't exist.
        /// </returns>
        /// <remarks>
        /// FindAsync() is optimized for looking up a single record by primary key.
        /// It's faster than using .Where() or .FirstOrDefaultAsync() for primary key lookups.
        /// </remarks>
        // GET: api/midpoints/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Midpoint>> GetMidpoint(int id)
        {
            // FindAsync is optimized for primary key lookups (very fast)
            var midpoint = await _context.Midpoints.FindAsync(id);

            // Return 404 Not Found if the midpoint doesn't exist (standard REST practice)
            if (midpoint == null)
            {
                return NotFound(); // Returns HTTP 404 status code
            }

            return midpoint; // Returns HTTP 200 OK with the midpoint data
        }

        /// <summary>
        /// Updates an existing midpoint's properties (name, status, description).
        /// Does not update the Location property - midpoints cannot be moved after creation.
        /// </summary>
        /// <param name="id">The unique ID of the midpoint to update (from the URL route)</param>
        /// <param name="midpoint">
        /// The updated midpoint data. Only Name, Status, and Description properties are updated.
        /// The Location property is ignored to prevent accidental movement of midpoints.
        /// </param>
        /// <returns>
        /// HTTP 204 No Content if successfully updated, HTTP 400 Bad Request if the ID doesn't match,
        /// or HTTP 404 Not Found if the midpoint doesn't exist.
        /// </returns>
        /// <remarks>
        /// RESTful Best Practice: PUT operations typically return 204 No Content (success with no body)
        /// when the update is successful, as the client already knows what was sent.
        /// 
        /// This method uses a partial update pattern - only the provided properties are updated.
        /// The Location property is explicitly preserved to prevent midpoints from being moved.
        /// 
        /// Entity Framework's Update() method marks all properties as modified, so we manually
        /// set only the properties we want to allow editing.
        /// </remarks>
        // PUT: api/midpoints/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMidpoint(int id, Midpoint midpoint)
        {
            // Ensure the ID in the URL matches the ID in the request body
            if (id != midpoint.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the request body.");
            }

            // Find the existing midpoint in the database
            var existingMidpoint = await _context.Midpoints.FindAsync(id);
            if (existingMidpoint == null)
            {
                return NotFound();
            }

            // Update only the editable properties (preserve Location and other system properties)
            existingMidpoint.Name = midpoint.Name;
            existingMidpoint.Status = midpoint.Status;
            existingMidpoint.Description = midpoint.Description;
            
            // Update Color property to match the new Status
            existingMidpoint.Color = GetStatusColor(midpoint.Status);
            
            // Note: Location is NOT updated - midpoints cannot be moved after creation

            // Mark the entity as modified so Entity Framework knows to update it
            _context.Entry(existingMidpoint).State = EntityState.Modified;

            try
            {
                // Execute the SQL UPDATE statement
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts (if another user modified the midpoint simultaneously)
                if (!await _context.Midpoints.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw; // Re-throw if it's a different concurrency issue
            }

            // Return 204 No Content (successful update with no response body)
            return NoContent();
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

        /// <summary>
        /// Helper method to get the color name string based on the midpoint status.
        /// Maps status colors: New (black), Review (light gray), Complete (light green), Issue (light red).
        /// </summary>
        /// <param name="status">The midpoint status to get the color for</param>
        /// <returns>A color name string that matches the status marker color</returns>
        private static string GetStatusColor(MidpointStatus status)
        {
            return status switch
            {
                MidpointStatus.New => "Black",
                MidpointStatus.Review => "LightGray",
                MidpointStatus.Complete => "LightGreen",
                MidpointStatus.Issue => "LightCoral",
                _ => "Black" // Default to black
            };
        }
    }
}