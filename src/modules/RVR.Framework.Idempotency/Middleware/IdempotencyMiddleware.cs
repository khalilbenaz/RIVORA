using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RVR.Framework.Idempotency.Services;

namespace RVR.Framework.Idempotency.Middleware;

/// <summary>
/// Middleware that provides idempotency by caching responses keyed by the X-Idempotency-Key header.
/// Only applies to POST and PATCH requests that include the header.
/// </summary>
public sealed class IdempotencyMiddleware
{
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyMiddleware"/> class.
    /// </summary>
    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the request, returning cached responses for duplicate idempotency keys.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IIdempotencyStore store)
    {
        if (!IsIdempotentMethod(context.Request.Method) ||
            !context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var keyValues))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = keyValues.ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        // Check for cached response
        var cached = await store.TryGetAsync(idempotencyKey, context.RequestAborted);
        if (cached is not null)
        {
            _logger.LogDebug("Returning cached idempotent response for key {Key}", idempotencyKey);
            context.Response.StatusCode = cached.StatusCode;
            if (cached.ContentType is not null)
                context.Response.ContentType = cached.ContentType;
            await context.Response.Body.WriteAsync(cached.Body, context.RequestAborted);
            return;
        }

        // Capture the response
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var bodyBytes = memoryStream.ToArray();

        // Cache the response
        var entry = new IdempotencyEntry
        {
            StatusCode = context.Response.StatusCode,
            Body = bodyBytes,
            ContentType = context.Response.ContentType
        };
        await store.SetAsync(idempotencyKey, entry, context.RequestAborted);

        // Write to original stream
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBodyStream, context.RequestAborted);
        context.Response.Body = originalBodyStream;
    }

    private static bool IsIdempotentMethod(string method) =>
        method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
        method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
}
