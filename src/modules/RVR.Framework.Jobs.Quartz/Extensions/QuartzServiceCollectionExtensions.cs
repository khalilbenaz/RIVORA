namespace RVR.Framework.Jobs.Quartz.Extensions;

using System;
using RVR.Framework.Jobs.Abstractions.Interfaces;
using RVR.Framework.Jobs.Abstractions.Options;
using RVR.Framework.Jobs.Quartz.Jobs;
using RVR.Framework.Jobs.Quartz.Services;
using Microsoft.Extensions.DependencyInjection;
using global::Quartz;
using global::Quartz.Impl;
using global::Quartz.Logging;

/// <summary>
/// Extension methods for configuring RIVORA Framework Jobs with Quartz.
/// </summary>
public static class QuartzServiceCollectionExtensions
{
    /// <summary>
    /// Adds RIVORA Framework Jobs with Quartz implementation using in-memory storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Quartz options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrQuartz(
        this IServiceCollection services,
        Action<JobSchedulerOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new JobSchedulerOptions
        {
            JobProvider = "Quartz",
            EnablePersistence = false
        };

        configureOptions?.Invoke(options);

        return services.AddRvrQuartzInternal(options);
    }

    /// <summary>
    /// Adds RIVORA Framework Jobs with Quartz implementation using database persistence.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string for Quartz storage.</param>
    /// <param name="configureOptions">Action to configure additional options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrQuartz(
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
            JobProvider = "Quartz",
            ConnectionString = connectionString,
            DbProvider = "SqlServer",
            EnablePersistence = true
        };

        configureOptions?.Invoke(options);

        return services.AddRvrQuartzInternal(options);
    }

    private static IServiceCollection AddRvrQuartzInternal(
        this IServiceCollection services,
        JobSchedulerOptions options)
    {
        services.AddSingleton(options);

        // Configure Quartz
        services.AddQuartz(configure =>
        {
            configure.UseSimpleTypeLoader();
            configure.UseDefaultThreadPool(pool =>
            {
                pool.MaxConcurrency = options.WorkerCount;
            });

            if (options.EnablePersistence && !string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                // Use database persistence
                configure.UsePersistentStore(store =>
                {
                    store.UseSqlServer(sqlOptions =>
                    {
                        sqlOptions.ConnectionString = options.ConnectionString!;
                        sqlOptions.TablePrefix = $"{options.SchemaName}.QRTZ_";
                    });

                    store.UseNewtonsoftJsonSerializer();
                    store.UseProperties = true;
                    store.UseClustering(c =>
                    {
                        c.CheckinInterval = TimeSpan.FromSeconds(options.PollingIntervalSeconds);
                    });
                });
            }
            else
            {
                // Use RAM job store
                configure.UseInMemoryStore();
            }

            // MicrosoftDependencyInjectionJobFactory is the default for DI configuration
        });

        // Add Quartz hosting
        services.AddQuartzHostedService(configure =>
        {
            configure.WaitForJobsToComplete = true;
            configure.StartDelay = TimeSpan.FromSeconds(1);
        });

        // Register the job wrapper
        services.AddTransient<KbaJobWrapper>();
        services.AddTransient<KbaRecurringJobWrapper>();

        // Register the job scheduler
        services.AddSingleton<IJobScheduler, QuartzJobScheduler>();

        return services;
    }
}
