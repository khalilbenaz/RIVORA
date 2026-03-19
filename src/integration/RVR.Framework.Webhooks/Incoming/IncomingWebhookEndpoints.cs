using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RVR.Framework.Webhooks.Incoming;

/// <summary>
/// Minimal API endpoint mappings for receiving and managing incoming webhooks.
/// </summary>
public static class IncomingWebhookEndpoints
{
    /// <summary>
    /// Maps all incoming webhook endpoints onto the given <see cref="WebApplication"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapIncomingWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Public receiver endpoint ────────────────────────────────────────
        app.MapPost("/api/webhooks/incoming/{source}", async (
            string source,
            HttpContext httpContext,
            InMemoryIncomingWebhookStore store) =>
        {
            // Read raw body
            httpContext.Request.EnableBuffering();
            using var reader = new StreamReader(httpContext.Request.Body);
            var payload = await reader.ReadToEndAsync();

            // Find matching config
            var config = store.GetConfigBySource(source);

            var log = new IncomingWebhookLog
            {
                Source = source,
                ConfigId = config?.Id ?? string.Empty,
                HttpMethod = httpContext.Request.Method,
                Headers = SerializeHeaders(httpContext.Request.Headers),
                Payload = payload,
                ReceivedAt = DateTime.UtcNow
            };

            // Try to extract event type from common headers
            log.EventType = TryGetEventType(httpContext.Request.Headers);

            if (config is null)
            {
                log.Status = "failed";
                log.Error = $"No active configuration found for source '{source}'.";
                log.StatusCode = 200; // Still return 200 to avoid retries from providers
                store.AddLog(log);

                return Results.Ok(new { received = true, warning = "no matching config" });
            }

            // Validate signature if configured
            if (!string.IsNullOrEmpty(config.Secret) && !string.IsNullOrEmpty(config.SignatureHeader))
            {
                var signatureValue = httpContext.Request.Headers[config.SignatureHeader].FirstOrDefault();
                if (string.IsNullOrEmpty(signatureValue))
                {
                    log.SignatureValid = false;
                    log.Status = "failed";
                    log.Error = $"Missing signature header '{config.SignatureHeader}'.";
                    log.StatusCode = 200;
                    store.AddLog(log);

                    return Results.Ok(new { received = true, signatureValid = false });
                }

                log.SignatureValid = IncomingWebhookSignatureValidator.Validate(
                    payload, config.Secret, signatureValue, config.SignatureAlgorithm);

                if (!log.SignatureValid)
                {
                    log.Status = "failed";
                    log.Error = "Signature validation failed.";
                    log.StatusCode = 200;
                    store.AddLog(log);

                    return Results.Ok(new { received = true, signatureValid = false });
                }
            }
            else
            {
                // No signature validation configured — mark as valid
                log.SignatureValid = true;
            }

            log.Status = "processed";
            log.StatusCode = 200;
            log.ProcessedAt = DateTime.UtcNow;
            store.AddLog(log);

            return Results.Ok(new { received = true, logId = log.Id });
        })
        .WithTags("IncomingWebhooks")
        .AllowAnonymous();

        // ── Management endpoints (authorized) ──────────────────────────────

        app.MapGet("/api/webhooks/incoming/configs", (InMemoryIncomingWebhookStore store) =>
        {
            return Results.Ok(store.GetConfigs());
        })
        .RequireAuthorization()
        .WithTags("IncomingWebhooks");

        app.MapPost("/api/webhooks/incoming/configs", (
            IncomingWebhookConfig config,
            InMemoryIncomingWebhookStore store) =>
        {
            // Ensure a fresh ID and timestamp
            config.Id = Guid.NewGuid().ToString("N");
            config.CreatedAt = DateTime.UtcNow;
            store.AddConfig(config);

            return Results.Created($"/api/webhooks/incoming/configs/{config.Id}", config);
        })
        .RequireAuthorization()
        .WithTags("IncomingWebhooks");

        app.MapDelete("/api/webhooks/incoming/configs/{id}", (
            string id,
            InMemoryIncomingWebhookStore store) =>
        {
            return store.RemoveConfig(id)
                ? Results.NoContent()
                : Results.NotFound();
        })
        .RequireAuthorization()
        .WithTags("IncomingWebhooks");

        app.MapGet("/api/webhooks/incoming/logs", (
            InMemoryIncomingWebhookStore store,
            int? limit) =>
        {
            return Results.Ok(store.GetAllLogs(limit ?? 50));
        })
        .RequireAuthorization()
        .WithTags("IncomingWebhooks");

        app.MapGet("/api/webhooks/incoming/logs/{id}", (
            string id,
            InMemoryIncomingWebhookStore store) =>
        {
            var log = store.GetLogById(id);
            return log is not null ? Results.Ok(log) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithTags("IncomingWebhooks");

        app.MapPost("/api/webhooks/incoming/logs/{id}/replay", (
            string id,
            InMemoryIncomingWebhookStore store) =>
        {
            var originalLog = store.GetLogById(id);
            if (originalLog is null)
                return Results.NotFound();

            // Create a new log entry as a replay of the original
            var replayLog = new IncomingWebhookLog
            {
                ConfigId = originalLog.ConfigId,
                Source = originalLog.Source,
                HttpMethod = originalLog.HttpMethod,
                EventType = originalLog.EventType,
                Headers = originalLog.Headers,
                Payload = originalLog.Payload,
                SignatureValid = originalLog.SignatureValid,
                Status = "processed",
                StatusCode = 200,
                ReceivedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            store.AddLog(replayLog);

            return Results.Ok(new { replayed = true, originalLogId = id, newLogId = replayLog.Id });
        })
        .RequireAuthorization()
        .WithTags("IncomingWebhooks");

        return app;
    }

    private static string SerializeHeaders(IHeaderDictionary headers)
    {
        var dict = new Dictionary<string, string>();
        foreach (var header in headers)
        {
            dict[header.Key] = header.Value.ToString();
        }
        return JsonSerializer.Serialize(dict);
    }

    private static string? TryGetEventType(IHeaderDictionary headers)
    {
        // Common event-type headers used by popular webhook providers
        if (headers.TryGetValue("X-GitHub-Event", out var githubEvent))
            return githubEvent.ToString();

        if (headers.TryGetValue("X-Stripe-Event", out var stripeEvent))
            return stripeEvent.ToString();

        if (headers.TryGetValue("X-Event-Type", out var genericEvent))
            return genericEvent.ToString();

        return null;
    }
}
