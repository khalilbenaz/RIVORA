namespace RVR.Framework.Security.OAuth;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

/// <summary>
/// Transforms external identity provider claims into standardized RVR Framework claims.
/// Maps provider-specific claim types for tenant ID, roles, and permissions
/// into a unified claim schema consumed by the framework's authorization pipeline.
/// </summary>
public class OAuthClaimsTransformer : IClaimsTransformation
{
    /// <summary>
    /// RVR claim type for tenant identifier.
    /// </summary>
    public const string RvrTenantIdClaim = "tenant_id";

    /// <summary>
    /// RVR claim type for permissions.
    /// </summary>
    public const string RvrPermissionClaim = "rvr:permission";

    /// <summary>
    /// Marker claim added after transformation to prevent duplicate processing.
    /// </summary>
    private const string TransformedMarker = "rvr:claims_transformed";

    private readonly ILogger<OAuthClaimsTransformer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthClaimsTransformer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public OAuthClaimsTransformer(ILogger<OAuthClaimsTransformer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        // Avoid transforming claims more than once per request.
        if (identity.HasClaim(c => c.Type == TransformedMarker))
        {
            return Task.FromResult(principal);
        }

        MapTenantId(identity);
        MapRoles(identity);
        MapPermissions(identity);

        identity.AddClaim(new Claim(TransformedMarker, "true"));

        _logger.LogDebug(
            "Transformed external claims for user {UserId}",
            identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");

        return Task.FromResult(principal);
    }

    /// <summary>
    /// Maps provider-specific tenant ID claims to the standard RVR tenant_id claim.
    /// </summary>
    private static void MapTenantId(ClaimsIdentity identity)
    {
        // Already has a tenant_id claim - nothing to do.
        if (identity.HasClaim(c => c.Type == RvrTenantIdClaim))
        {
            return;
        }

        // Azure AD: tid claim carries the tenant identifier.
        var tenantClaim = identity.FindFirst("tid")
            // Keycloak: tenant may be in a custom attribute or the azp (authorized party).
            ?? identity.FindFirst("tenant")
            // Auth0: custom namespace claim (e.g., https://myapp.com/tenant_id).
            ?? identity.FindFirst(c => c.Type.EndsWith("/tenant_id", StringComparison.OrdinalIgnoreCase))
            // OIDC standard: some providers put it in the azp claim.
            ?? identity.FindFirst("azp");

        if (tenantClaim is not null)
        {
            identity.AddClaim(new Claim(RvrTenantIdClaim, tenantClaim.Value));
        }
    }

    /// <summary>
    /// Maps provider-specific role claims to standard ClaimTypes.Role claims.
    /// </summary>
    private static void MapRoles(ClaimsIdentity identity)
    {
        // Keycloak embeds roles inside realm_access/resource_access as JSON,
        // but the OIDC middleware typically flattens them into "realm_roles" or "role" claims.
        // We normalise any non-standard role claim types to ClaimTypes.Role.

        string[] roleClaimTypes =
        [
            "roles",                          // Azure AD v2.0
            "realm_roles",                    // Keycloak (flattened)
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", // already standard
            "https://schemas.auth0.com/roles", // Auth0 custom namespace
        ];

        var existingRoles = new HashSet<string>(
            identity.FindAll(ClaimTypes.Role).Select(c => c.Value),
            StringComparer.OrdinalIgnoreCase);

        foreach (var type in roleClaimTypes)
        {
            if (type == ClaimTypes.Role)
            {
                continue;
            }

            foreach (var claim in identity.FindAll(type))
            {
                if (existingRoles.Add(claim.Value))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                }
            }
        }
    }

    /// <summary>
    /// Maps provider-specific permission claims to the RVR permission claim type.
    /// </summary>
    private static void MapPermissions(ClaimsIdentity identity)
    {
        string[] permissionClaimTypes =
        [
            "permissions",                          // Auth0
            "permission",                           // Generic
            "scp",                                  // Azure AD (scope)
            "scope",                                // OIDC standard scope
            "https://schemas.auth0.com/permissions", // Auth0 custom namespace
        ];

        var existingPermissions = new HashSet<string>(
            identity.FindAll(RvrPermissionClaim).Select(c => c.Value),
            StringComparer.OrdinalIgnoreCase);

        foreach (var type in permissionClaimTypes)
        {
            foreach (var claim in identity.FindAll(type))
            {
                // Scopes and permissions may be space-delimited.
                var values = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var value in values)
                {
                    if (existingPermissions.Add(value))
                    {
                        identity.AddClaim(new Claim(RvrPermissionClaim, value));
                    }
                }
            }
        }
    }
}
