namespace KBA.Framework.Jobs.Abstractions.Extensions;

using System;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using KBA.Framework.Jobs.Abstractions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring KBA Framework Jobs.
/// </summary>
public static class JobsServiceCollectionExtensions
{
    /// <summary>
    /// Adds KBA Framework Jobs with configuration from appsettings.
    /// Reads "JobProvider" setting to determine which implementation to use.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read settings from.</param>
    /// <param name="configureOptions">Action to configure additional options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaJobs(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JobSchedulerOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var options = new JobSchedulerOptions();
        configuration.GetSection("JobScheduler").Bind(options);
        configureOptions?.Invoke(options);

        services.Configure<JobSchedulerOptions>(configuration.GetSection("JobScheduler"));

        return services.AddKbaJobs(options);
    }

    /// <summary>
    /// Adds KBA Framework Jobs with the specified options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaJobs(
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

        var options = new JobSchedulerOptions();
        configureOptions(options);

        return services.AddKbaJobs(options);
    }

    /// <summary>
    /// Adds KBA Framework Jobs with the specified options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The job scheduler options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaJobs(
        this IServiceCollection services,
        JobSchedulerOptions options)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton(options);
        services.AddSingleton<IJobScheduler>(sp =>
        {
            var schedulerOptions = sp.GetRequiredService<JobSchedulerOptions>();
            return schedulerOptions.JobProvider.ToLowerInvariant() switch
            {
                "hangfire" => throw new InvalidOperationException(
                    "Hangfire provider not registered. Use AddKbaHangfire() instead."),
                "quartz" => throw new InvalidOperationException(
                    "Quartz provider not registered. Use AddKbaQuartz() instead."),
                _ => throw new InvalidOperationException(
                    $"Unknown job provider: {schedulerOptions.JobProvider}. Use 'Hangfire' or 'Quartz'.")
            };
        });

        return services;
    }

    /// <summary>
    /// Gets the job scheduler from the service provider.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>The job scheduler.</returns>
    public static IJobScheduler GetJobScheduler(this IServiceProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        return provider.GetRequiredService<IJobScheduler>();
    }
}
