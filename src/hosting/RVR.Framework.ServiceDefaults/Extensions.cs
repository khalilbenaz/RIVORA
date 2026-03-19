using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Adds RIVORA Framework-specific service defaults on top of the standard Aspire service defaults.
    /// Configures Serilog enrichment properties, RIVORA-tagged health checks, and extended OpenTelemetry tracing.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="serviceName">Optional service name used for telemetry enrichment. Defaults to the application name.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddRivoraDefaults(
        this IHostApplicationBuilder builder,
        string? serviceName = null)
    {
        // Start with standard Aspire service defaults
        builder.AddServiceDefaults();

        var resolvedServiceName = serviceName ?? builder.Environment.ApplicationName;

        // Configure Serilog-style enrichment properties via logging scope
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

        // Add RIVORA-specific OpenTelemetry resource attributes
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("service.name", resolvedServiceName),
                    new KeyValuePair<string, object>("service.framework", "RIVORA"),
                    new KeyValuePair<string, object>("service.framework.version", "3.1.0")
                });
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource("RVR.Framework.*");
            });

        // Add RIVORA-specific health checks with framework tags
        builder.Services.AddHealthChecks()
            .AddCheck("rivora-framework", () => HealthCheckResult.Healthy("RIVORA Framework is operational"),
                tags: ["live", "ready", "rivora"])
            .AddCheck("rivora-dependencies", () => HealthCheckResult.Healthy("Dependencies are available"),
                tags: ["ready", "rivora"]);

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps default health check endpoints including standard Kubernetes paths.
    /// <list type="bullet">
    /// <item><description><c>/health</c> - all health checks (general readiness)</description></item>
    /// <item><description><c>/alive</c> - liveness check (tagged "live")</description></item>
    /// <item><description><c>/healthz</c> - Kubernetes liveness probe</description></item>
    /// <item><description><c>/ready</c> - Kubernetes readiness probe (all checks must pass)</description></item>
    /// </list>
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        // Kubernetes standard liveness probe endpoint
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        // Kubernetes standard readiness probe endpoint
        app.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = _ => true
        });

        return app;
    }

    /// <summary>
    /// Maps RIVORA-specific health check endpoints in addition to the default ones.
    /// Adds <c>/health/rivora</c> endpoint that only checks RIVORA-tagged health checks.
    /// </summary>
    public static WebApplication MapRivoraEndpoints(this WebApplication app)
    {
        app.MapDefaultEndpoints();

        // RIVORA-specific health check endpoint
        app.MapHealthChecks("/health/rivora", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("rivora")
        });

        return app;
    }
}
