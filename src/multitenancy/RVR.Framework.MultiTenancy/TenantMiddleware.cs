namespace RVR.Framework.MultiTenancy;

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;

/// <summary>
/// ASP.NET Core middleware for resolving the current tenant from the HTTP request.
/// Validates that the tenant exists via <see cref="ITenantStore"/> (when registered)
/// and verifies the tenant matches the authenticated user's TenantId claim.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    private readonly string _tenantHeaderName;
    private readonly string _tenantQueryStringName;

    public const string TenantKey = "__TenantInfo__";
    public const string TenantIdClaimType = "TenantId";

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger, string tenantHeaderName = "X-Tenant-Id", string tenantQueryStringName = "tenant")
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantHeaderName = tenantHeaderName;
        _tenantQueryStringName = tenantQueryStringName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = ResolveTenantId(context);

        if (string.IsNullOrEmpty(tenantId))
        {
            // No tenant specified - continue without tenant context
            await _next(context);
            return;
        }

        // Validate tenant exists via ITenantStore (if registered)
        var tenantStore = context.RequestServices.GetService<ITenantStore>();
        TenantInfo? tenantInfo;

        if (tenantStore != null)
        {
            tenantInfo = await tenantStore.GetTenantAsync(tenantId);
            if (tenantInfo == null)
            {
                _logger.LogWarning("Tenant '{TenantId}' not found or inactive", LogSanitizer.Sanitize(tenantId));
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync($"Tenant '{tenantId}' is not valid or not active.");
                return;
            }
        }
        else
        {
            // No store registered - create basic info (backward compatible)
            tenantInfo = new TenantInfo { Id = tenantId, Name = tenantId, Identifier = tenantId };
        }

        // For authenticated requests, verify the tenant matches the user's JWT TenantId claim
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claimTenantId = context.User.FindFirstValue(TenantIdClaimType);
            if (!string.IsNullOrEmpty(claimTenantId) &&
                !string.Equals(claimTenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Tenant ID mismatch: header/query '{RequestTenantId}' does not match JWT claim '{ClaimTenantId}'",
                    LogSanitizer.Sanitize(tenantId), LogSanitizer.Sanitize(claimTenantId));
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Tenant ID does not match authenticated user's tenant.");
                return;
            }
        }

        context.Items[TenantKey] = tenantInfo;
        await _next(context);
    }

    private string? ResolveTenantId(HttpContext context)
    {
        var tenantId = context.Request.Headers[_tenantHeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId)) tenantId = context.Request.Query[_tenantQueryStringName].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId))
        {
            var host = context.Request.Host.Host;
            var parts = host.Split(".");
            if (parts.Length > 2) tenantId = parts[0];
        }
        return tenantId;
    }
}

/// <summary>
/// Extension methods for adding the TenantMiddleware to the ASP.NET Core pipeline.
/// </summary>
public static class TenantMiddlewareExtensions
{
    /// <summary>
    /// Adds the tenant middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseTenancy(this IApplicationBuilder app) => app.UseMiddleware<TenantMiddleware>();
}
