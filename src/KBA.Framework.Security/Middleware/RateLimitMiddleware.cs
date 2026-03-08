namespace KBA.Framework.Security.Middleware;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KBA.Framework.Security.Interfaces;
using KBA.Framework.Security.Models;
using KBA.Framework.Security.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// ASP.NET Core middleware for rate limiting HTTP requests.
/// Supports multiple strategies: Fixed Window, Sliding Window, and Token Bucket.
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitService _rateLimitService;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="rateLimitService">The rate limit service.</param>
    /// <param name="options">The rate limit options.</param>
    /// <param name="logger">The logger.</param>
    public RateLimitMiddleware(
        RequestDelegate next,
        IRateLimitService rateLimitService,
        IOptions<RateLimitOptions> options,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware for an HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        // Check if admin bypass is enabled and user is admin
        if (_options.BypassForAdmins && IsAdminUser(context))
        {
            await _next(context);
            return;
        }

        var endpoint = context.Request.Path.Value ?? "/";
        var ipAddress = GetClientIpAddress(context);
        var userId = GetUserId(context);
        var tenantId = GetTenantId(context);

        var result = await _rateLimitService.CheckAsync(
            endpoint,
            ipAddress,
            userId,
            tenantId,
            context.RequestAborted);

        if (_options.IncludeHeaders)
        {
            AddRateLimitHeaders(context.Response, result);
        }

        if (!result.IsAllowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded for {Endpoint} from {IpAddress}. Rule: {RuleName}",
                endpoint, ipAddress, result.RuleName);

            context.Response.StatusCode = 429;
            context.Response.Headers.RetryAfter = result.RetryAfterSeconds?.ToString() ?? "60";

            var message = result.RuleName != null
                ? $"Rate limit exceeded: {result.RuleName}"
                : "Too many requests. Please try again later.";

            await context.Response.WriteAsJsonAsync(new
            {
                error = message,
                retryAfter = result.RetryAfterSeconds,
                resetAt = result.ResetAt
            });

            return;
        }

        await _next(context);
    }

    private bool IsAdminUser(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return context.User.HasClaim(c =>
            c.Type == ClaimTypes.Role && c.Value == "Admin") ||
            context.User.HasClaim(c =>
            c.Type == "role" && c.Value == "Admin");
    }

    private string? GetUserId(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = context.User.FindFirst(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "sub" ||
            c.Type == "user_id");

        return userIdClaim?.Value;
    }

    private string? GetTenantId(HttpContext context)
    {
        var tenantIdClaim = context.User?.FindFirst(c =>
            c.Type == "tenant_id" ||
            c.Type == "tid");

        return tenantIdClaim?.Value;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for X-Forwarded-For header (for proxied requests)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the list
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();

        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void AddRateLimitHeaders(HttpResponse response, RateLimitResult result)
    {
        response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
        response.Headers["X-RateLimit-Reset"] = result.ResetAt.ToString();

        if (!result.IsAllowed && result.RetryAfterSeconds.HasValue)
        {
            response.Headers["Retry-After"] = result.RetryAfterSeconds.Value.ToString();
        }
    }
}

/// <summary>
/// Extension methods for adding rate limiting middleware to ASP.NET Core applications.
/// </summary>
public static class RateLimitMiddlewareExtensions
{
    /// <summary>
    /// Adds the rate limiting middleware to the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<RateLimitMiddleware>();
    }

    /// <summary>
    /// Adds rate limiting services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure rate limit options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        Action<RateLimitOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<RateLimitOptions>()
            .Configure(configureOptions ?? (_ => { }))
            .ValidateDataAnnotations();

        services.AddSingleton<IRateLimitService, RateLimitService>();

        return services;
    }

    /// <summary>
    /// Adds rate limiting services with in-memory store to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure rate limit options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRateLimitingWithInMemoryStore(
        this IServiceCollection services,
        Action<RateLimitOptions>? configureOptions = null)
    {
        services.AddRateLimiting(configureOptions);
        services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();

        return services;
    }
}
