namespace RVR.Framework.Jobs.Quartz.Jobs;

using System;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Jobs.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Quartz job wrapper that executes RIVORA Framework IJob implementations.
/// </summary>
public class KbaJobWrapper : global::Quartz.IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KbaJobWrapper> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KbaJobWrapper"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public KbaJobWrapper(IServiceProvider serviceProvider, ILogger<KbaJobWrapper> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task Execute(global::Quartz.IJobExecutionContext context)
    {
        var jobType = context.JobDetail.JobType;

        _logger.LogInformation("Executing Quartz job {JobType} with key {JobKey}",
            jobType.Name, context.JobDetail.Key);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var job = (IJob)scope.ServiceProvider.GetRequiredService(jobType);
            
            context.MergedJobDataMap.Put("StartedAt", DateTime.UtcNow);
            
            await job.ExecuteAsync(context.CancellationToken).ConfigureAwait(false);
            
            context.MergedJobDataMap.Put("CompletedAt", DateTime.UtcNow);
            
            _logger.LogInformation("Quartz job {JobType} completed successfully", jobType.Name);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Quartz job {JobType} was cancelled", jobType.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quartz job {JobType} failed with error: {Error}", jobType.Name, ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Quartz job wrapper for recurring jobs.
/// </summary>
public class KbaRecurringJobWrapper : global::Quartz.IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KbaRecurringJobWrapper> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KbaRecurringJobWrapper"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public KbaRecurringJobWrapper(IServiceProvider serviceProvider, ILogger<KbaRecurringJobWrapper> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task Execute(global::Quartz.IJobExecutionContext context)
    {
        var jobType = context.JobDetail.JobType;

        _logger.LogInformation("Executing recurring Quartz job {JobType} with key {JobKey}", 
            jobType.Name, context.JobDetail.Key);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var job = (IRecurringJob)scope.ServiceProvider.GetRequiredService(jobType);
            
            context.MergedJobDataMap.Put("StartedAt", (object)DateTime.UtcNow);
            context.MergedJobDataMap.Put("FireTime", (object)context.FireTimeUtc.UtcDateTime);
            context.MergedJobDataMap.Put("ScheduledFireTime", (object?)context.ScheduledFireTimeUtc?.UtcDateTime ?? string.Empty);
            
            await job.ExecuteAsync(context.CancellationToken).ConfigureAwait(false);
            
            context.MergedJobDataMap.Put("CompletedAt", (object)DateTime.UtcNow);
            context.MergedJobDataMap.Put("PreviousFireTime", (object?)context.PreviousFireTimeUtc?.UtcDateTime ?? string.Empty);
            context.MergedJobDataMap.Put("NextFireTime", (object?)context.NextFireTimeUtc?.UtcDateTime ?? string.Empty);
            
            _logger.LogInformation("Recurring Quartz job {JobType} completed successfully", jobType.Name);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Recurring Quartz job {JobType} was cancelled", jobType.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recurring Quartz job {JobType} failed with error: {Error}", jobType.Name, ex.Message);
            throw;
        }
    }
}
