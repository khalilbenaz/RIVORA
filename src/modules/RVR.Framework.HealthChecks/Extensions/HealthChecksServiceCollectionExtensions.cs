namespace RVR.Framework.HealthChecks.Extensions;

using System;
using System.Collections.Generic;
using RVR.Framework.HealthChecks.Checks;
using RVR.Framework.HealthChecks.Writers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Extension methods for configuring RIVORA Framework Health Checks.
/// </summary>
public static class HealthChecksServiceCollectionExtensions
{
    /// <summary>
    /// Adds all RIVORA Framework Health Checks to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure health check options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrHealthChecks(
        this IServiceCollection services,
        Action<HealthCheckOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new HealthCheckOptions();
        configureOptions?.Invoke(options);

        services.AddOptions<HealthCheckOptions>()
            .Configure(configureOptions ?? (_ => { }));

        // Add basic health checks
        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "ready" });

        return services;
    }

    /// <summary>
    /// Adds database health checks to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="databaseType">The type of database.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags for filtering health checks.</param>
    /// <param name="timeout">The timeout for the health check.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDatabaseHealthCheck(
        this IServiceCollection services,
        string connectionString,
        string databaseType = "Unknown",
        string name = "database",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));
        }

        var dbRegistration = new HealthCheckRegistration(
            name,
            sp => new DatabaseHealthCheck(connectionString, databaseType),
            null,
            tags ?? new[] { "database", "ready" });
        if (timeout.HasValue) dbRegistration.Timeout = timeout.Value;
        services.AddHealthChecks().Add(dbRegistration);

        return services;
    }

    /// <summary>
    /// Adds Entity Framework Core health check to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags for filtering health checks.</param>
    /// <param name="timeout">The timeout for the health check.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEfCoreHealthCheck<TContext>(
        this IServiceCollection services,
        string name = "database-efcore",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var efRegistration = new HealthCheckRegistration(
            name,
            sp => new EfCoreHealthCheck(sp, typeof(TContext)),
            null,
            tags ?? new[] { "database", "efcore", "ready" });
        if (timeout.HasValue) efRegistration.Timeout = timeout.Value;
        services.AddHealthChecks().Add(efRegistration);

        return services;
    }

    /// <summary>
    /// Adds Redis health check to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags for filtering health checks.</param>
    /// <param name="timeout">The timeout for the health check.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRedisHealthCheck(
        this IServiceCollection services,
        string connectionString,
        string name = "redis",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));
        }

        var redisRegistration = new HealthCheckRegistration(
            name,
            sp => new RedisHealthCheck(connectionString),
            null,
            tags ?? new[] { "cache", "redis", "ready" });
        if (timeout.HasValue) redisRegistration.Timeout = timeout.Value;
        services.AddHealthChecks().Add(redisRegistration);

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ health check to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The RabbitMQ connection string.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags for filtering health checks.</param>
    /// <param name="timeout">The timeout for the health check.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRabbitMqHealthCheck(
        this IServiceCollection services,
        string connectionString,
        string name = "rabbitmq",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));
        }

        var rabbitRegistration = new HealthCheckRegistration(
            name,
            sp => new RabbitMqHealthCheck(connectionString),
            null,
            tags ?? new[] { "messaging", "rabbitmq", "ready" });
        if (timeout.HasValue) rabbitRegistration.Timeout = timeout.Value;
        services.AddHealthChecks().Add(rabbitRegistration);

        return services;
    }

    /// <summary>
    /// Adds AI provider health check to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="providerName">The name of the AI provider.</param>
    /// <param name="apiKey">The API key.</param>
    /// <param name="endpoint">The API endpoint.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags for filtering health checks.</param>
    /// <param name="timeout">The timeout for the health check.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAiProviderHealthCheck(
        this IServiceCollection services,
        string providerName,
        string apiKey,
        string endpoint,
        string name = "ai-provider",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHttpClient(name)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });

        var aiRegistration = new HealthCheckRegistration(
            name,
            sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new AiProviderHealthCheck(providerName, apiKey, endpoint, httpClientFactory.CreateClient(name), timeout);
            },
            null,
            tags ?? new[] { "ai", providerName.ToLowerInvariant(), "ready" });
        if (timeout.HasValue) aiRegistration.Timeout = timeout.Value;
        services.AddHealthChecks().Add(aiRegistration);

        return services;
    }

    /// <summary>
    /// Adds jobs health check to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="jobMonitor">The job health monitor.</param>
    /// <param name="criticalQueues">The critical queues to monitor.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags for filtering health checks.</param>
    /// <param name="timeout">The timeout for the health check.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddJobsHealthCheck(
        this IServiceCollection services,
        IJobHealthMonitor jobMonitor,
        IEnumerable<string>? criticalQueues = null,
        string name = "jobs",
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (jobMonitor == null)
        {
            throw new ArgumentNullException(nameof(jobMonitor));
        }

        services.AddSingleton(jobMonitor);

        var jobsRegistration = new HealthCheckRegistration(
            name,
            sp => new JobsHealthCheck(
                sp.GetRequiredService<IJobHealthMonitor>(),
                criticalQueues),
            null,
            tags ?? new[] { "jobs", "background", "ready" });
        if (timeout.HasValue) jobsRegistration.Timeout = timeout.Value;
        services.AddHealthChecks().Add(jobsRegistration);

        return services;
    }

    /// <summary>
    /// Configures health check endpoints with custom response writers.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="healthPath">The path for the health endpoint.</param>
    /// <param name="readyPath">The path for the readiness endpoint.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRvrHealthChecks(
        this IApplicationBuilder app,
        string healthPath = "/health",
        string readyPath = "/health/ready")
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        // Main health endpoint
        app.UseHealthChecks(healthPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteHealthCheckResponseAsync,
            AllowCachingResponses = false
        });

        // Readiness endpoint
        app.UseHealthChecks(readyPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteReadinessResponseAsync,
            Predicate = check => check.Tags.Contains("ready"),
            AllowCachingResponses = false
        });

        return app;
    }

    /// <summary>
    /// Configures detailed health check endpoint.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="path">The path for the detailed health endpoint.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRvrDetailedHealthChecks(
        this IApplicationBuilder app,
        string path = "/health/detailed")
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.UseHealthChecks(path, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = (context, result) => HealthCheckResponseWriter.WriteDetailedHealthResponseAsync(context, result, true),
            AllowCachingResponses = false
        });

        return app;
    }
}

/// <summary>
/// Configuration options for RIVORA Framework Health Checks.
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// Gets or sets whether to include database health checks.
    /// </summary>
    public bool IncludeDatabase { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include Redis health checks.
    /// </summary>
    public bool IncludeRedis { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include RabbitMQ health checks.
    /// </summary>
    public bool IncludeRabbitMq { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include AI provider health checks.
    /// </summary>
    public bool IncludeAiProviders { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include jobs health checks.
    /// </summary>
    public bool IncludeJobs { get; set; } = true;

    /// <summary>
    /// Gets or sets the default timeout for health checks.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the health endpoint path.
    /// </summary>
    public string HealthPath { get; set; } = "/health";

    /// <summary>
    /// Gets or sets the readiness endpoint path.
    /// </summary>
    public string ReadyPath { get; set; } = "/health/ready";

    /// <summary>
    /// Gets or sets whether to enable detailed health responses.
    /// </summary>
    public bool EnableDetailedResponses { get; set; } = false;
}
