namespace KBA.Framework.Caching.Middleware;

using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KBA.Framework.Caching.Attributes;
using KBA.Framework.Caching.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Middleware for handling response caching with KbaCache attribute.
/// </summary>
public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IResponseCacheService _cacheService;
    private readonly ILogger<ResponseCachingMiddleware>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseCachingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="logger">The logger.</param>
    public ResponseCachingMiddleware(
        RequestDelegate next,
        IResponseCacheService cacheService,
        ILogger<ResponseCachingMiddleware>? logger = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware for the current request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var kbaCacheAttribute = endpoint?.Metadata.GetMetadata<KbaCacheAttribute>();

        if (kbaCacheAttribute == null)
        {
            await _next(context);
            return;
        }

        // Check HTTP method
        if (!kbaCacheAttribute.HttpMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Check authentication bypass
        if (kbaCacheAttribute.BypassForAuthenticatedUsers && context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        // Generate cache key
        var cacheKey = GenerateCacheKey(context, kbaCacheAttribute);

        // Try to get from cache
        var cachedResponse = await _cacheService.GetAsync<CachedResponse>(cacheKey);

        if (cachedResponse != null)
        {
            _logger?.LogDebug("Cache hit for key: {Key}", cacheKey);
            await WriteCachedResponseAsync(context, cachedResponse);
            return;
        }

        _logger?.LogDebug("Cache miss for key: {Key}", cacheKey);

        // Capture the response
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new System.IO.MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        // Check if we should cache
        if (ShouldCacheResponse(context, kbaCacheAttribute))
        {
            memoryStream.Position = 0;
            var responseBody = await new System.IO.StreamReader(memoryStream).ReadToEndAsync();

            var cachedResponseToStore = new CachedResponse
            {
                Body = responseBody,
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                CachedAt = DateTime.UtcNow
            };

            await _cacheService.SetAsync(
                cacheKey,
                cachedResponseToStore,
                kbaCacheAttribute.Duration,
                kbaCacheAttribute.Tags);

            _logger?.LogDebug("Response cached for key: {Key}", cacheKey);

            // Write back to original response
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }

        context.Response.Body = originalBodyStream;
    }

    private string GenerateCacheKey(HttpContext context, KbaCacheAttribute attribute)
    {
        var keyBuilder = new StringBuilder();

        // Add key prefix if specified
        if (!string.IsNullOrWhiteSpace(attribute.KeyPrefix))
        {
            keyBuilder.Append(attribute.KeyPrefix);
        }

        // Add custom key if specified
        if (!string.IsNullOrWhiteSpace(attribute.Key))
        {
            keyBuilder.Append(attribute.Key);
        }
        else
        {
            // Generate key from request
            keyBuilder.Append(context.Request.Path);

            // Add query string if varying by query
            if (attribute.VaryByQuery)
            {
                var query = context.Request.Query;
                var queryParams = attribute.VaryByQueryParams?.Length > 0
                    ? query.Where(q => attribute.VaryByQueryParams.Contains(q.Key))
                    : query;

                foreach (var param in queryParams.OrderBy(q => q.Key))
                {
                    keyBuilder.Append($":{param.Key}={param.Value}");
                }
            }

            // Add headers if varying by header
            if (attribute.VaryByHeader && attribute.VaryByHeaders?.Length > 0)
            {
                foreach (var header in attribute.VaryByHeaders.OrderBy(h => h))
                {
                    if (context.Request.Headers.TryGetValue(header, out var values))
                    {
                        keyBuilder.Append($":{header}={values}");
                    }
                }
            }

            // Add user identity if varying by user
            if (attribute.VaryByUser && context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             context.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    keyBuilder.Append($":user={userId}");
                }
            }
        }

        return keyBuilder.ToString();
    }

    private bool ShouldCacheResponse(HttpContext context, KbaCacheAttribute attribute)
    {
        // Check if only caching successful responses
        if (attribute.CacheOnlySuccess && (context.Response.StatusCode < 200 || context.Response.StatusCode >= 300))
        {
            return false;
        }

        // Don't cache if response has no-cache header
        if (context.Response.Headers.ContainsKey("no-cache"))
        {
            return false;
        }

        return true;
    }

    private async Task WriteCachedResponseAsync(HttpContext context, CachedResponse cachedResponse)
    {
        context.Response.StatusCode = cachedResponse.StatusCode;
        context.Response.ContentType = cachedResponse.ContentType ?? "application/json";
        context.Response.Headers["X-Cache"] = "HIT";
        context.Response.Headers["X-Cache-At"] = cachedResponse.CachedAt.ToString("O");

        await context.Response.WriteAsync(cachedResponse.Body);
    }
}

/// <summary>
/// Represents a cached response.
/// </summary>
public class CachedResponse
{
    /// <summary>
    /// Gets or sets the response body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the time when the response was cached.
    /// </summary>
    public DateTime CachedAt { get; set; }
}
