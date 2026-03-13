using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace KBA.Framework.MultiTenancy;

/// <summary>
/// Implementation of ITenantProvider that resolves the current tenant from the HttpContext.
/// </summary>
public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public TenantInfo? GetCurrentTenant()
    {
        return _httpContextAccessor.HttpContext?.Items[TenantMiddleware.TenantKey] as TenantInfo;
    }

    /// <inheritdoc />
    public string? GetConnectionString()
    {
        var tenant = GetCurrentTenant();

        // If the tenant has a specific connection string, use it.
        if (tenant != null && !string.IsNullOrEmpty(tenant.ConnectionString))
        {
            return tenant.ConnectionString;
        }

        // Fallback to the default connection string.
        return _configuration.GetConnectionString("DefaultConnection");
    }
}
