namespace RVR.Framework.HealthChecks.Writers;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RVR.Framework.HealthChecks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Custom health check response writer that produces formatted JSON responses.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Writes a health check result as JSON to the HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="result">The health check result.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport result)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        context.Response.ContentType = "application/json";

        var healthStatus = new Models.HealthStatus
        {
            Status = result.Status.ToString().ToLowerInvariant(),
            TotalDuration = result.TotalDuration,
            Results = MapHealthCheckEntries(result.Entries)
        };

        var statusCode = result.Status switch
        {
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy => StatusCodes.Status200OK,
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded => StatusCodes.Status200OK,
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            healthStatus,
            JsonOptions);
    }

    /// <summary>
    /// Writes a readiness check result as JSON to the HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="result">The health check result.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteReadinessResponseAsync(HttpContext context, HealthReport result)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        context.Response.ContentType = "application/json";

        var notReadyChecks = result.Entries
            .Where(e => e.Value.Status != Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
            .Select(e => e.Key)
            .ToList();

        var readinessStatus = new ReadinessStatus
        {
            IsReady = result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy,
            NotReadyChecks = notReadyChecks,
            Results = MapHealthCheckEntries(result.Entries)
        };

        context.Response.StatusCode = result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            readinessStatus,
            JsonOptions);
    }

    /// <summary>
    /// Writes a detailed health check report with additional metadata.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="result">The health check result.</param>
    /// <param name="includeDetails">Whether to include detailed information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteDetailedHealthResponseAsync(
        HttpContext context,
        HealthReport result,
        bool includeDetails = true)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        context.Response.ContentType = "application/json";

        var response = new
        {
            status = result.Status.ToString().ToLowerInvariant(),
            totalDuration = result.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow.ToString("O"),
            version = GetAssemblyVersion(),
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            checks = MapHealthCheckEntriesDetailed(result.Entries, includeDetails)
        };

        var statusCode = result.Status switch
        {
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy => StatusCodes.Status200OK,
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded => StatusCodes.Status200OK,
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            JsonOptions);
    }

    private static IEnumerable<Models.HealthCheckResult> MapHealthCheckEntries(
        IReadOnlyDictionary<string, HealthReportEntry> entries)
    {
        return entries.Select(entry => new Models.HealthCheckResult
        {
            Name = entry.Key,
            Status = entry.Value.Status.ToString().ToLowerInvariant(),
            Duration = entry.Value.Duration,
            Exception = entry.Value.Exception?.Message,
            Description = entry.Value.Description,
            Tags = entry.Value.Tags,
            Data = entry.Value.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new()
        });
    }

    private static IEnumerable<object> MapHealthCheckEntriesDetailed(
        IReadOnlyDictionary<string, HealthReportEntry> entries,
        bool includeDetails)
    {
        return entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString().ToLowerInvariant(),
            duration = entry.Value.Duration.TotalMilliseconds,
            description = entry.Value.Description,
            exception = entry.Value.Exception?.Message,
            tags = entry.Value.Tags,
            data = includeDetails ? entry.Value.Data : null
        });
    }

    private static string GetAssemblyVersion()
    {
        var assembly = typeof(HealthCheckResponseWriter).Assembly;
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "unknown";
    }
}
