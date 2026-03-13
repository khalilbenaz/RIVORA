namespace KBA.Framework.MultiTenancy;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// ASP.NET Core middleware for resolving the current tenant from the HTTP request.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    private readonly string _tenantHeaderName;
    private readonly string _tenantQueryStringName;

    public const string TenantKey = "__TenantInfo__";

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger, string tenantHeaderName = "X-Tenant-Id", string tenantQueryStringName = "tenant")
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantHeaderName = tenantHeaderName;
        _tenantQueryStringName = tenantQueryStringName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantInfo = await ResolveTenantAsync(context);
        if (tenantInfo != null) context.Items[TenantKey] = tenantInfo;
        await _next(context);
    }

    private Task<TenantInfo?> ResolveTenantAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers[_tenantHeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId)) tenantId = context.Request.Query[_tenantQueryStringName].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantId))
        {
            var host = context.Request.Host.Host;
            var parts = host.Split(".");
            if (parts.Length > 2) tenantId = parts[0];
        }
        if (string.IsNullOrEmpty(tenantId)) return Task.FromResult<TenantInfo?>(null);
        return Task.FromResult<TenantInfo?>(new TenantInfo { Id = tenantId, Name = tenantId, Identifier = tenantId });
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
