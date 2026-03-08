namespace KBA.Framework.Jobs.Quartz.Extensions;

using System;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using KBA.Framework.Jobs.Abstractions.Options;
using KBA.Framework.Jobs.Quartz.Jobs;
using KBA.Framework.Jobs.Quartz.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

/// <summary>
/// Extension methods for configuring KBA Framework Jobs with Quartz.
/// </summary>
public static class QuartzServiceCollectionExtensions
{
    /// <summary>
    /// Adds KBA Framework Jobs with Quartz implementation using in-memory storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Quartz options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaQuartz(
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

        return services.AddKbaQuartzInternal(options);
    }

    /// <summary>
    /// Adds KBA Framework Jobs with Quartz implementation using database persistence.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string for Quartz storage.</param>
    /// <param name="configureOptions">Action to configure additional options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaQuartz(
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

        return services.AddKbaQuartzInternal(options);
    }

    /// <summary>
    /// Adds KBA Framework Jobs with Quartz implementation using configured options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Quartz options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaQuartz(
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
            JobProvider = "Quartz"
        };

        configureOptions(options);

        return services.AddKbaQuartzInternal(options);
    }

    private static IServiceCollection AddKbaQuartzInternal(
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
                    store.UseSqlServer(options.ConnectionString, sqlOptions =>
                    {
                        sqlOptions.TablePrefix = $"{options.SchemaName}.QRTZ_";
                        sqlOptions.UseClustering = true;
                        sqlOptions.ClusterCheckinInterval = TimeSpan.FromSeconds(options.PollingIntervalSeconds);
                    });

                    store.UseNewtonsoftJsonSerializer();
                    store.UseProperties = true;
                });
            }
            else
            {
                // Use RAM job store
                configure.UseInMemoryStore();
            }

            // Configure retry policy
            configure.UseMicrosoftDependencyInjectionJobFactory();
            configure.UseSchedulingCoordinator(s =>
            {
                s.PollingInterval = TimeSpan.FromSeconds(options.PollingIntervalSeconds);
            });
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
