using System.Security.Claims;
using KBA.Framework.Application.Services;
using Microsoft.AspNetCore.Http;

namespace KBA.Framework.Infrastructure.Services;

/// <summary>
/// Implémentation du contexte utilisateur basée sur HttpContext et JWT Claims
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
            return tenantIdClaim != null && Guid.TryParse(tenantIdClaim, out var tenantId)
                ? tenantId
                : null;
        }
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null && Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }
    }

    /// <inheritdoc />
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
