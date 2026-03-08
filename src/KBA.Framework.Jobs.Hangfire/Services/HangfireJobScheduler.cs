namespace KBA.Framework.Jobs.Hangfire.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using KBA.Framework.Jobs.Abstractions.Interfaces;
using KBA.Framework.Jobs.Abstractions.Models;
using KBA.Framework.Jobs.Abstractions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Hangfire implementation of the IJobScheduler interface.
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly JobSchedulerOptions _options;
    private readonly ILogger<HangfireJobScheduler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HangfireJobScheduler"/> class.
    /// </summary>
    /// <param name="backgroundJobClient">The Hangfire background job client.</param>
    /// <param name="recurringJobManager">The Hangfire recurring job manager.</param>
    /// <param name="options">The job scheduler options.</param>
    /// <param name="logger">The logger.</param>
    public HangfireJobScheduler(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IOptions<JobSchedulerOptions> options,
        ILogger<HangfireJobScheduler> logger)
    {
        _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string ProviderName => "Hangfire";

    /// <inheritdoc/>
    public Task<JobInfo> EnqueueAsync<TJob>(CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        return EnqueueAsync<TJob>(new Dictionary<string, string>(), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<JobInfo> EnqueueAsync<TJob>(
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobInfo = CreateJobInfo<TJob>(parameters);
        
        _logger.LogInformation("Enqueueing job {JobType} with ID {JobId}", typeof(TJob).Name, jobInfo.Id);

        var jobId = _backgroundJobClient.Enqueue<TJob>(x => x.ExecuteAsync(cancellationToken));
        jobInfo.Id = jobId;
        jobInfo.Status = JobStatus.Pending;

        return Task.FromResult(jobInfo);
    }

    /// <inheritdoc/>
    public Task<JobInfo> ScheduleAsync<TJob>(
        DateTimeOffset executeAt,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        return ScheduleAsync<TJob>(executeAt, new Dictionary<string, string>(), cancellationToken);
    }

    /// <summary>
    /// Schedules a job with parameters for future execution.
    /// </summary>
    /// <typeparam name="TJob">The type of job to schedule.</typeparam>
    /// <param name="executeAt">The time when the job should execute.</param>
    /// <param name="parameters">Parameters to pass to the job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information including the job ID.</returns>
    public Task<JobInfo> ScheduleAsync<TJob>(
        DateTimeOffset executeAt,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobInfo = CreateJobInfo<TJob>(parameters);
        
        _logger.LogInformation("Scheduling job {JobType} for {ExecuteAt} with ID {JobId}", 
            typeof(TJob).Name, executeAt, jobInfo.Id);

        var jobId = _backgroundJobClient.Schedule<TJob>(
            x => x.ExecuteAsync(cancellationToken),
            executeAt.DateTime);
        
        jobInfo.Id = jobId;
        jobInfo.Status = JobStatus.Scheduled;
        jobInfo.NextExecution = executeAt.UtcDateTime;

        return Task.FromResult(jobInfo);
    }

    /// <inheritdoc/>
    public Task<JobInfo> RegisterRecurringAsync<TJob>(
        string recurringJobId,
        string cronExpression,
        string timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
        where TJob : IRecurringJob
    {
        return RegisterRecurringAsync<TJob>(recurringJobId, cronExpression, new Dictionary<string, string>(), timeZoneId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<JobInfo> RegisterRecurringAsync<TJob>(
        string recurringJobId,
        string cronExpression,
        IDictionary<string, string> parameters,
        string timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
        where TJob : IRecurringJob
    {
        if (string.IsNullOrWhiteSpace(recurringJobId))
        {
            throw new ArgumentException("Recurring job ID cannot be empty.", nameof(recurringJobId));
        }

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException("Cron expression cannot be empty.", nameof(cronExpression));
        }

        var jobInfo = CreateJobInfo<TJob>(parameters);
        jobInfo.Id = recurringJobId;
        jobInfo.CronExpression = cronExpression;
        jobInfo.IsRecurring = true;

        _logger.LogInformation("Registering recurring job {JobId} with cron {CronExpression}", 
            recurringJobId, cronExpression);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        
        await _recurringJobManager.AddOrUpdateAsync<TJob>(
            recurringJobId,
            x => x.ExecuteAsync(cancellationToken),
            cronExpression,
            timeZone).ConfigureAwait(false);

        jobInfo.Status = JobStatus.Scheduled;

        return jobInfo;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveRecurringAsync(string recurringJobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recurringJobId))
        {
            throw new ArgumentException("Recurring job ID cannot be empty.", nameof(recurringJobId));
        }

        _logger.LogInformation("Removing recurring job {JobId}", recurringJobId);

        await _recurringJobManager.RemoveAsync(recurringJobId).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task<JobInfo?> GetJobInfoAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new ArgumentException("Job ID cannot be empty.", nameof(jobId));
        }

        var jobDetails = await Task.FromResult(_backgroundJobClient.GetJobDetails(jobId)).ConfigureAwait(false);
        if (jobDetails == null)
        {
            return null;
        }

        return MapToJobInfo(jobId, jobDetails);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JobInfo>> GetRecurringJobsAsync(CancellationToken cancellationToken = default)
    {
        var recurringJobs = await Task.FromResult(_recurringJobManager.GetRecurringJobs()).ConfigureAwait(false);
        
        return recurringJobs.Select(rj => new JobInfo
        {
            Id = rj.Id,
            Name = rj.Job?.Type?.Name ?? rj.Id,
            JobType = rj.Job?.Type?.FullName ?? string.Empty,
            Status = JobStatus.Scheduled,
            CronExpression = rj.Cron,
            IsRecurring = true,
            NextExecution = rj.NextExecution?.UtcDateTime
        });
    }

    /// <inheritdoc/>
    public async Task<bool> CancelAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new ArgumentException("Job ID cannot be empty.", nameof(jobId));
        }

        _logger.LogInformation("Cancelling job {JobId}", jobId);

        await Task.FromResult(_backgroundJobClient.Delete(jobId)).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return await CancelAsync(jobId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<JobStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var statistics = new JobStatistics
        {
            Pending = (int)_backgroundJobClient.GetJobCount("Enqueued"),
            Processing = (int)_backgroundJobClient.GetJobCount("Processing"),
            Scheduled = (int)_backgroundJobClient.GetJobCount("Scheduled"),
            Failed = (int)_backgroundJobClient.GetJobCount("Failed"),
            Succeeded = (int)_backgroundJobClient.GetJobCount("Succeeded"),
            Recurring = _recurringJobManager.GetRecurringJobs().Count()
        };

        return Task.FromResult(statistics);
    }

    private JobInfo CreateJobInfo<TJob>(IDictionary<string, string> parameters)
    {
        return new JobInfo
        {
            Name = typeof(TJob).Name,
            JobType = typeof(TJob).FullName ?? string.Empty,
            Status = JobStatus.Pending,
            MaxRetries = _options.DefaultMaxRetries,
            Queue = _options.DefaultQueue,
            Data = parameters.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(parameters) : null
        };
    }

    private JobInfo MapToJobInfo(string jobId, dynamic jobDetails)
    {
        var state = jobDetails.State;
        var status = state?.Name?.ToLowerInvariant() switch
        {
            "enqueued" => JobStatus.Pending,
            "processing" => JobStatus.Running,
            "succeeded" => JobStatus.Completed,
            "failed" => JobStatus.Failed,
            "scheduled" => JobStatus.Scheduled,
            "deleted" => JobStatus.Cancelled,
            _ => JobStatus.Pending
        };

        return new JobInfo
        {
            Id = jobId,
            Name = jobDetails.Job?.Type?.Name ?? jobId,
            JobType = jobDetails.Job?.Type?.FullName ?? string.Empty,
            Status = status,
            CreatedAt = jobDetails.CreatedAt?.UtcDateTime ?? DateTime.UtcNow,
            StartedAt = jobDetails.LoadedAt?.UtcDateTime,
            CompletedAt = state?.Name?.ToLowerInvariant() == "succeeded" ? DateTime.UtcNow : null,
            ErrorMessage = state?.Name?.ToLowerInvariant() == "failed" ? state?.Reason : null,
            RetryCount = 0,
            MaxRetries = _options.DefaultMaxRetries
        };
    }
}
