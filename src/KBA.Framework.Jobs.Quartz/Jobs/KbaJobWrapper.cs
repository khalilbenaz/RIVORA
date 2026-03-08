namespace KBA.Framework.Jobs.Quartz.Jobs;

using System;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Quartz job wrapper that executes KBA Framework IJob implementations.
/// </summary>
public class KbaJobWrapper : Quartz.IJob
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
    public async Task Execute(Quartz.IJobExecutionContext context)
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
public class KbaRecurringJobWrapper : Quartz.IJob
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
    public async Task Execute(Quartz.IJobExecutionContext context)
    {
        var jobType = context.JobDetail.JobType;
        
        _logger.LogInformation("Executing recurring Quartz job {JobType} with key {JobKey}", 
            jobType.Name, context.JobDetail.Key);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var job = (IRecurringJob)scope.ServiceProvider.GetRequiredService(jobType);
            
            context.MergedJobDataMap.Put("StartedAt", DateTime.UtcNow);
            context.MergedJobDataMap.Put("FireTime", context.FireTimeUtc.UtcDateTime);
            context.MergedJobDataMap.Put("ScheduledFireTime", context.ScheduledFireTimeUtc?.UtcDateTime);
            
            await job.ExecuteAsync(context.CancellationToken).ConfigureAwait(false);
            
            context.MergedJobDataMap.Put("CompletedAt", DateTime.UtcNow);
            context.MergedJobDataMap.Put("PreviousFireTime", context.PreviousFireTimeUtc?.UtcDateTime);
            context.MergedJobDataMap.Put("NextFireTime", context.NextFireTimeUtc?.UtcDateTime);
            
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
