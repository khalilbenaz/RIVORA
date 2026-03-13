namespace KBA.Framework.Api.Middleware;

/// <summary>
/// Middleware pour ajouter des en-têtes de sécurité (OWASP Default 4.5)
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Content-Security-Policy
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");

        // HTTP Strict Transport Security (HSTS)
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // X-Frame-Options
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-Content-Type-Options
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Referrer-Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        await _next(context);
    }
}
