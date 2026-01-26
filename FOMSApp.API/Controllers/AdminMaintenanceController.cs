using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FOMSApp.API.Services;

namespace FOMSApp.API.Controllers;

/// <summary>
/// Maintenance endpoints intended for admin use.
/// Protected by Azure AD authentication - requires Admin role.
/// </summary>
[Route("api/admin")]
[ApiController]
[Authorize(Policy = "RequireAdmin")]
public class AdminMaintenanceController(DeletedEntitiesPurger purger, IConfiguration configuration) : ControllerBase
{
    private readonly DeletedEntitiesPurger _purger = purger;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Permanently purges soft-deleted entities older than the configured retention window.
    /// Deletes DB rows and their associated photo files.
    /// </summary>
    /// <param name="retentionDays">Optional override (back-compat). If provided, uses X days.</param>
    /// <param name="retention">Optional override (preferred). TimeSpan format, e.g. "00:01:00" for 1 minute.</param>
    [HttpPost("purge-deleted")]
    public async Task<ActionResult<PurgeDeletedResult>> PurgeDeleted(
        [FromQuery] int? retentionDays,
        [FromQuery] TimeSpan? retention,
        CancellationToken cancellationToken)
    {
        // Resolve retention window:
        // 1) explicit query override
        // 2) back-compat query override (days)
        // 3) configuration (TimeSpan)
        // 4) fallback 10 days
        var resolvedRetention =
            retention
            ?? (retentionDays.HasValue ? TimeSpan.FromDays(retentionDays.Value) : (TimeSpan?)null)
            ?? _configuration.GetValue<TimeSpan?>("Retention:PurgeDeletedAfter")
            ?? TimeSpan.FromDays(10);

        var result = await _purger.PurgeAsync(resolvedRetention, cancellationToken);
        return Ok(result);
    }
}

