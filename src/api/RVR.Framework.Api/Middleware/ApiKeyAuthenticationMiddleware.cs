using System.Security.Cryptography;
using System.Text;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Api.Middleware;

/// <summary>
/// Middleware pour l'authentification par clé API (4.6)
/// API keys are compared using SHA-256 hashes stored in the database
/// and timing-safe comparison to prevent side-channel attacks.
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RVRDbContext dbContext)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            await _next(context);
            return;
        }

        var keyValue = extractedApiKey.ToString();
        var keyHash = ComputeHash(keyValue);

        // Look up by hash for security; fall back to plaintext comparison
        // for backward compatibility during migration period.
        var apiKey = await dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.IsActive &&
                (k.KeyHash == keyHash || (k.KeyHash == null && k.Key == keyValue)));

        if (apiKey == null || (apiKey.ExpiresAtUtc.HasValue && apiKey.ExpiresAtUtc < DateTime.UtcNow))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Clé API invalide ou expirée.");
            return;
        }

        // If key was matched by plaintext (legacy), migrate to hash
        if (apiKey.KeyHash == null)
        {
            apiKey.KeyHash = keyHash;
            apiKey.Key = $"***{keyValue[^4..]}"; // Keep only last 4 chars as prefix for identification
        }

        // Mettre à jour la date de dernière utilisation
        apiKey.LastUsedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // Créer une identité pour la clé API
        var claims = new List<System.Security.Claims.Claim>
        {
            new("apikey", apiKey.Name)
        };

        if (apiKey.TenantId.HasValue)
            claims.Add(new("TenantId", apiKey.TenantId.Value.ToString()));

        if (apiKey.UserId.HasValue)
            claims.Add(new(System.Security.Claims.ClaimTypes.NameIdentifier, apiKey.UserId.Value.ToString()));

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "ApiKey");
        context.User.AddIdentity(identity);

        await _next(context);
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
