namespace RVR.Framework.Jobs.Quartz.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Jobs.Abstractions;
using RVR.Framework.Jobs.Abstractions.Interfaces;
using RVR.Framework.Jobs.Abstractions.Models;
using RVR.Framework.Jobs.Abstractions.Options;
using IJob = RVR.Framework.Jobs.Abstractions.Interfaces.IJob;
using RVR.Framework.Jobs.Quartz.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using global::Quartz;
using global::Quartz.Impl;
using global::Quartz.Impl.Matchers;

/// <summary>
/// Quartz implementation of the IJobScheduler interface.
/// </summary>
public class QuartzJobScheduler : IJobScheduler
{
    private readonly IScheduler _scheduler;
    private readonly JobSchedulerOptions _options;
    private readonly ILogger<QuartzJobScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuartzJobScheduler"/> class.
    /// </summary>
    /// <param name="scheduler">The Quartz scheduler.</param>
    /// <param name="options">The job scheduler options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public QuartzJobScheduler(
        IScheduler scheduler,
        IOptions<JobSchedulerOptions> options,
        ILogger<QuartzJobScheduler> logger,
        IServiceProvider serviceProvider)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public string ProviderName => "Quartz";

    /// <inheritdoc/>
    public async Task<JobInfo> EnqueueAsync<TJob>(CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        return await EnqueueAsync<TJob>(new Dictionary<string, string>(), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<JobInfo> EnqueueAsync<TJob>(
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobInfo = CreateJobInfo<TJob>(parameters);
        
        _logger.LogInformation("Enqueueing Quartz job {JobType} with ID {JobId}", typeof(TJob).Name, jobInfo.Id);

        var jobKey = new JobKey(jobInfo.Id, _options.DefaultQueue);
        var jobDetail = JobBuilder.Create<KbaJobWrapper>()
            .WithIdentity(jobKey)
            .WithDescription(typeof(TJob).FullName)
            .Build();

        jobDetail.JobDataMap.Put("JobType", typeof(TJob).AssemblyQualifiedName);
        jobDetail.JobDataMap.Put("Parameters", System.Text.Json.JsonSerializer.Serialize(parameters));

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobInfo.Id}-trigger", _options.DefaultQueue)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).ConfigureAwait(false);

        jobInfo.Status = JobStatus.Pending;
        return jobInfo;
    }

    /// <inheritdoc/>
    public async Task<JobInfo> ScheduleAsync<TJob>(
        DateTimeOffset executeAt,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        return await ScheduleAsync<TJob>(executeAt, new Dictionary<string, string>(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Schedules a job with parameters for future execution.
    /// </summary>
    public async Task<JobInfo> ScheduleAsync<TJob>(
        DateTimeOffset executeAt,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobInfo = CreateJobInfo<TJob>(parameters);
        
        _logger.LogInformation("Scheduling Quartz job {JobType} for {ExecuteAt} with ID {JobId}", 
            typeof(TJob).Name, executeAt, jobInfo.Id);

        var jobKey = new JobKey(jobInfo.Id, _options.DefaultQueue);
        var jobDetail = JobBuilder.Create<KbaJobWrapper>()
            .WithIdentity(jobKey)
            .WithDescription(typeof(TJob).FullName)
            .Build();

        jobDetail.JobDataMap.Put("JobType", typeof(TJob).AssemblyQualifiedName);
        jobDetail.JobDataMap.Put("Parameters", System.Text.Json.JsonSerializer.Serialize(parameters));

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobInfo.Id}-trigger", _options.DefaultQueue)
            .StartAt(executeAt)
            .Build();

        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).ConfigureAwait(false);

        jobInfo.Status = JobStatus.Scheduled;
        jobInfo.NextExecution = executeAt.UtcDateTime;

        return jobInfo;
    }

    /// <inheritdoc/>
    public async Task<JobInfo> RegisterRecurringAsync<TJob>(
        string recurringJobId,
        string cronExpression,
        string timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
        where TJob : IRecurringJob
    {
        return await RegisterRecurringAsync<TJob>(recurringJobId, cronExpression, new Dictionary<string, string>(), timeZoneId, cancellationToken).ConfigureAwait(false);
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

        _logger.LogInformation("Registering recurring Quartz job {JobId} with cron {CronExpression}", 
            recurringJobId, cronExpression);

        var jobKey = new JobKey(recurringJobId, _options.DefaultQueue);
        
        // Check if job already exists and remove it
        if (await _scheduler.CheckExists(jobKey, cancellationToken).ConfigureAwait(false))
        {
            await _scheduler.DeleteJob(jobKey, cancellationToken).ConfigureAwait(false);
        }

        var jobDetail = JobBuilder.Create<KbaRecurringJobWrapper>()
            .WithIdentity(jobKey)
            .WithDescription(typeof(TJob).FullName)
            .Build();

        jobDetail.JobDataMap.Put("JobType", typeof(TJob).AssemblyQualifiedName);
        jobDetail.JobDataMap.Put("Parameters", System.Text.Json.JsonSerializer.Serialize(parameters));

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var cronSchedule = CronScheduleBuilder.CronSchedule(cronExpression)
            .InTimeZone(timeZone);

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{recurringJobId}-trigger", _options.DefaultQueue)
            .WithSchedule(cronSchedule)
            .Build();

        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken).ConfigureAwait(false);

        jobInfo.Status = JobStatus.Scheduled;
        jobInfo.NextExecution = trigger.GetNextFireTimeUtc()?.UtcDateTime;

        return jobInfo;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveRecurringAsync(string recurringJobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recurringJobId))
        {
            throw new ArgumentException("Recurring job ID cannot be empty.", nameof(recurringJobId));
        }

        _logger.LogInformation("Removing recurring Quartz job {JobId}", recurringJobId);

        var jobKey = new JobKey(recurringJobId, _options.DefaultQueue);
        await _scheduler.DeleteJob(jobKey, cancellationToken).ConfigureAwait(false);
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<JobInfo?> GetJobInfoAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new ArgumentException("Job ID cannot be empty.", nameof(jobId));
        }

        var jobKey = new JobKey(jobId, _options.DefaultQueue);
        
        if (!await _scheduler.CheckExists(jobKey, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var jobDetail = await _scheduler.GetJobDetail(jobKey, cancellationToken).ConfigureAwait(false);
        var triggers = await _scheduler.GetTriggersOfJob(jobKey, cancellationToken).ConfigureAwait(false);
        var trigger = triggers.FirstOrDefault();

        return MapToJobInfo(jobId, jobDetail, trigger);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JobInfo>> GetRecurringJobsAsync(CancellationToken cancellationToken = default)
    {
        var jobKeys = await _scheduler.GetJobKeys(
            GroupMatcher<JobKey>.AnyGroup(), 
            cancellationToken).ConfigureAwait(false);

        var recurringJobs = new List<JobInfo>();

        foreach (var jobKey in jobKeys)
        {
            var jobDetail = await _scheduler.GetJobDetail(jobKey, cancellationToken).ConfigureAwait(false);
            if (jobDetail == null) continue;
            var triggers = await _scheduler.GetTriggersOfJob(jobKey, cancellationToken).ConfigureAwait(false);

            foreach (var trigger in triggers)
            {
                if (trigger is ICronTrigger cronTrigger)
                {
                    recurringJobs.Add(new JobInfo
                    {
                        Id = jobKey.Name,
                        Name = jobDetail.Description ?? jobKey.Name,
                        JobType = jobDetail.JobType.FullName ?? string.Empty,
                        Status = JobStatus.Scheduled,
                        CronExpression = cronTrigger.CronExpressionString,
                        IsRecurring = true,
                        NextExecution = trigger.GetNextFireTimeUtc()?.UtcDateTime
                    });
                }
            }
        }

        return recurringJobs;
    }

    /// <inheritdoc/>
    public async Task<bool> CancelAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new ArgumentException("Job ID cannot be empty.", nameof(jobId));
        }

        _logger.LogInformation("Cancelling Quartz job {JobId}", jobId);

        var jobKey = new JobKey(jobId, _options.DefaultQueue);
        await _scheduler.Interrupt(jobKey, cancellationToken).ConfigureAwait(false);
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new ArgumentException("Job ID cannot be empty.", nameof(jobId));
        }

        _logger.LogInformation("Deleting Quartz job {JobId}", jobId);

        var jobKey = new JobKey(jobId, _options.DefaultQueue);
        await _scheduler.DeleteJob(jobKey, cancellationToken).ConfigureAwait(false);
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<JobStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var schedulerMetaData = await _scheduler.GetMetaData().ConfigureAwait(false);
        
        var jobKeys = await _scheduler.GetJobKeys(
            GroupMatcher<JobKey>.AnyGroup(), 
            cancellationToken).ConfigureAwait(false);

        var statistics = new JobStatistics
        {
            Recurring = (await GetRecurringJobsAsync(cancellationToken).ConfigureAwait(false)).Count()
        };

        // Count jobs by state
        foreach (var jobKey in jobKeys)
        {
            var triggers = await _scheduler.GetTriggersOfJob(jobKey, cancellationToken).ConfigureAwait(false);
            foreach (var trigger in triggers)
            {
                var state = await _scheduler.GetTriggerState(trigger.Key, cancellationToken).ConfigureAwait(false);
                switch (state)
                {
                    case TriggerState.Normal:
                    case TriggerState.Blocked:
                        statistics.Pending++;
                        break;
                    case TriggerState.Paused:
                        statistics.Scheduled++;
                        break;
                    case TriggerState.Complete:
                        statistics.Succeeded++;
                        break;
                    case TriggerState.Error:
                        statistics.Failed++;
                        break;
                }
            }
        }

        var currentlyExecutingJobs = await _scheduler.GetCurrentlyExecutingJobs(cancellationToken).ConfigureAwait(false);
        statistics.Processing = currentlyExecutingJobs.Count;

        return statistics;
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

    private JobInfo MapToJobInfo(string jobId, IJobDetail? jobDetail, ITrigger? trigger)
    {
        var status = trigger?.GetNextFireTimeUtc() switch
        {
            null => JobStatus.Completed,
            _ => JobStatus.Scheduled
        };

        return new JobInfo
        {
            Id = jobId,
            Name = jobDetail?.Description ?? jobId,
            JobType = jobDetail?.JobType.FullName ?? string.Empty,
            Status = status,
            CronExpression = (trigger as ICronTrigger)?.CronExpressionString,
            IsRecurring = trigger is ICronTrigger,
            NextExecution = trigger?.GetNextFireTimeUtc()?.UtcDateTime,
            MaxRetries = _options.DefaultMaxRetries
        };
    }
}
