namespace RVR.Framework.Api.Middleware;

using System.Security.Cryptography;
using Microsoft.IO;

public class ETagMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly RecyclableMemoryStreamManager _streamManager = new();

    public ETagMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only for GET/HEAD requests
        if (context.Request.Method != HttpMethods.Get && context.Request.Method != HttpMethods.Head)
        {
            await _next(context);
            return;
        }

        var originalStream = context.Response.Body;
        await using var memoryStream = _streamManager.GetStream();
        context.Response.Body = memoryStream;

        await _next(context);

        // Only for 200 OK responses
        if (context.Response.StatusCode == 200)
        {
            memoryStream.Position = 0;
            var bodyBytes = memoryStream.ToArray();
            var etag = GenerateETag(bodyBytes);

            context.Response.Headers.ETag = etag;
            context.Response.Headers.CacheControl = "no-cache";

            // Check If-None-Match
            if (context.Request.Headers.IfNoneMatch == etag)
            {
                context.Response.StatusCode = 304;
                context.Response.ContentLength = 0;
                return;
            }

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalStream);
        }
        else
        {
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalStream);
        }
    }

    private static string GenerateETag(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return $"\"{Convert.ToBase64String(hash[..16])}\"";
    }
}
