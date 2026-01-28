using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace FOMSApp.API.Services;

/// <summary>
/// Transforms Azure AD "roles" claims to standard ClaimTypes.Role claims.
/// This runs on every authenticated request to ensure role-based authorization works.
/// </summary>
public class RoleClaimsTransformation : IClaimsTransformation
{
    private readonly ILogger<RoleClaimsTransformation> _logger;

    public RoleClaimsTransformation(ILogger<RoleClaimsTransformation> logger)
    {
        _logger = logger;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Clone the principal to avoid modifying the original
        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        // Check if we already have standard role claims
        var hasStandardRoleClaims = claimsIdentity.Claims
            .Any(c => c.Type == ClaimTypes.Role);

        if (hasStandardRoleClaims)
        {
            return Task.FromResult(principal);
        }

        // Get Azure AD "roles" claims and add them as standard role claims
        var azureRoleClaims = claimsIdentity.Claims
            .Where(c => c.Type == "roles")
            .ToList();

        foreach (var roleClaim in azureRoleClaims)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            _logger.LogDebug("Added role claim: {Role}", roleClaim.Value);
        }

        if (azureRoleClaims.Any())
        {
            _logger.LogInformation("Transformed {Count} Azure AD roles to standard claims for user {User}", 
                azureRoleClaims.Count, principal.Identity?.Name);
        }

        return Task.FromResult(principal);
    }
}
