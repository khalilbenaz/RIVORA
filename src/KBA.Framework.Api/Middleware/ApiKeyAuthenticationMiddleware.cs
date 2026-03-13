using KBA.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KBA.Framework.Api.Middleware;

/// <summary>
/// Middleware pour l'authentification par clé API (4.6)
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, KBADbContext dbContext)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            await _next(context);
            return;
        }

        var apiKey = await dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == extractedApiKey.ToString() && k.IsActive);

        if (apiKey == null || (apiKey.ExpiresAtUtc.HasValue && apiKey.ExpiresAtUtc < DateTime.UtcNow))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Clé API invalide ou expirée.");
            return;
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
}
