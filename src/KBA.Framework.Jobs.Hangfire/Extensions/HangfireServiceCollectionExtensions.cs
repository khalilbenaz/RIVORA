namespace KBA.Framework.Jobs.Hangfire.Extensions;

using System;
using Hangfire;
using KBA.Framework.Jobs.Abstractions.Options;
using KBA.Framework.Jobs.Hangfire.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods for configuring KBA Framework Jobs with Hangfire.
/// </summary>
public static class HangfireServiceCollectionExtensions
{
    /// <summary>
    /// Adds KBA Framework Jobs with Hangfire implementation using SQL Server storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string for Hangfire storage.</param>
    /// <param name="configureOptions">Action to configure additional options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaHangfire(
        this IServiceCollection services,
        string connectionString,
        Action<JobSchedulerOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));
        }

        var options = new JobSchedulerOptions
        {
            JobProvider = "Hangfire",
            ConnectionString = connectionString,
            DbProvider = "SqlServer"
        };

        configureOptions?.Invoke(options);

        return services.AddKbaHangfireInternal(options);
    }

    /// <summary>
    /// Adds KBA Framework Jobs with Hangfire implementation using configured options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Hangfire options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaHangfire(
        this IServiceCollection services,
        Action<JobSchedulerOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        var options = new JobSchedulerOptions
        {
            JobProvider = "Hangfire"
        };

        configureOptions(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("ConnectionString must be configured for Hangfire.");
        }

        return services.AddKbaHangfireInternal(options);
    }

    /// <summary>
    /// Adds Hangfire dashboard middleware to the pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseKbaHangfireDashboard(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var options = app.ApplicationServices.GetService<IOptions<JobSchedulerOptions>>()?.Value;
        var dashboardPath = options?.DashboardPath ?? "/hangfire";

        app.UseHangfireDashboard(dashboardPath);

        return app;
    }

    /// <summary>
    /// Adds Hangfire dashboard middleware with authentication.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="authorizationFilter">The authorization filter for dashboard access.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseKbaHangfireDashboard(
        this IApplicationBuilder app,
        Hangfire.Dashboard.IDashboardAuthorizationFilter authorizationFilter)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var options = app.ApplicationServices.GetService<IOptions<JobSchedulerOptions>>()?.Value;
        var dashboardPath = options?.DashboardPath ?? "/hangfire";

        app.UseHangfireDashboard(dashboardPath, new DashboardOptions
        {
            Authorization = new[] { authorizationFilter }
        });

        return app;
    }

    private static IServiceCollection AddKbaHangfireInternal(
        this IServiceCollection services,
        JobSchedulerOptions options)
    {
        services.AddSingleton(options);

        // Configure Hangfire storage
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(options.ConnectionString, new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(options.PollingIntervalSeconds),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                    SchemaName = options.SchemaName
                });

            // Configure retry strategy
            config.UseFilter(new AutomaticRetryAttribute
            {
                Attempts = options.DefaultMaxRetries,
                OnAttemptsExceeded = AttemptsExceededAction.Delete,
                LogEvents = true
            });
        });

        // Add Hangfire processing server
        services.AddHangfireServer(config =>
        {
            config.ServerName = options.ServerName;
            config.Queues = new[] { options.DefaultQueue, "default" };
            config.WorkerCount = options.WorkerCount;
            config.SchedulePollingInterval = TimeSpan.FromSeconds(options.PollingIntervalSeconds);
        });

        // Register the job scheduler
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();

        return services;
    }
}

/// <summary>
/// Simple authorization filter that allows all authenticated users.
/// </summary>
public class AllowAuthenticatedDashboardFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    /// <inheritdoc/>
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}

/// <summary>
/// Authorization filter that allows specific roles.
/// </summary>
public class RoleBasedDashboardFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleBasedDashboardFilter"/> class.
    /// </summary>
    /// <param name="allowedRoles">The roles allowed to access the dashboard.</param>
    public RoleBasedDashboardFilter(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles ?? Array.Empty<string>();
    }

    /// <inheritdoc/>
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        foreach (var role in _allowedRoles)
        {
            if (httpContext.User.IsInRole(role))
            {
                return true;
            }
        }

        return false;
    }
}
