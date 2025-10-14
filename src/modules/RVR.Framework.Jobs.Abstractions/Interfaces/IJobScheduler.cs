namespace RVR.Framework.Jobs.Abstractions.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Jobs.Abstractions.Models;

/// <summary>
/// Defines the contract for job scheduling operations across different providers.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Gets the name of the job provider (e.g., "Hangfire", "Quartz").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Enqueues a job for immediate execution.
    /// </summary>
    /// <typeparam name="TJob">The type of job to enqueue.</typeparam>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information including the job ID.</returns>
    Task<JobInfo> EnqueueAsync<TJob>(CancellationToken cancellationToken = default)
        where TJob : IJob;

    /// <summary>
    /// Enqueues a job with parameters for immediate execution.
    /// </summary>
    /// <typeparam name="TJob">The type of job to enqueue.</typeparam>
    /// <param name="parameters">Parameters to pass to the job.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information including the job ID.</returns>
    Task<JobInfo> EnqueueAsync<TJob>(
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
        where TJob : IJob;

    /// <summary>
    /// Schedules a job for future execution.
    /// </summary>
    /// <typeparam name="TJob">The type of job to schedule.</typeparam>
    /// <param name="executeAt">The time when the job should execute.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information including the job ID.</returns>
    Task<JobInfo> ScheduleAsync<TJob>(
        DateTimeOffset executeAt,
        CancellationToken cancellationToken = default)
        where TJob : IJob;

    /// <summary>
    /// Registers a recurring job using a cron expression.
    /// </summary>
    /// <typeparam name="TJob">The type of recurring job.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job.</param>
    /// <param name="cronExpression">The cron expression defining the schedule.</param>
    /// <param name="timeZoneId">The timezone for the cron schedule.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information.</returns>
    Task<JobInfo> RegisterRecurringAsync<TJob>(
        string recurringJobId,
        string cronExpression,
        string timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
        where TJob : IRecurringJob;

    /// <summary>
    /// Registers a recurring job with parameters.
    /// </summary>
    /// <typeparam name="TJob">The type of recurring job.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job.</param>
    /// <param name="cronExpression">The cron expression defining the schedule.</param>
    /// <param name="parameters">Parameters to pass to the job.</param>
    /// <param name="timeZoneId">The timezone for the cron schedule.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information.</returns>
    Task<JobInfo> RegisterRecurringAsync<TJob>(
        string recurringJobId,
        string cronExpression,
        IDictionary<string, string> parameters,
        string timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
        where TJob : IRecurringJob;

    /// <summary>
    /// Removes a recurring job registration.
    /// </summary>
    /// <param name="recurringJobId">The ID of the recurring job to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the job was removed.</returns>
    Task<bool> RemoveRecurringAsync(string recurringJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about a job by its ID.
    /// </summary>
    /// <param name="jobId">The job ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The job information, or null if not found.</returns>
    Task<JobInfo?> GetJobInfoAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all recurring jobs.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of recurring job information.</returns>
    Task<IEnumerable<JobInfo>> GetRecurringJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending or scheduled job.
    /// </summary>
    /// <param name="jobId">The job ID to cancel.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the job was cancelled.</returns>
    Task<bool> CancelAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a job from the system.
    /// </summary>
    /// <param name="jobId">The job ID to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the job was deleted.</returns>
    Task<bool> DeleteAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets job statistics.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Job statistics.</returns>
    Task<JobStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents job scheduling statistics.
/// </summary>
public class JobStatistics
{
    /// <summary>
    /// Gets or sets the number of pending jobs.
    /// </summary>
    public int Pending { get; set; }

    /// <summary>
    /// Gets or sets the number of jobs currently executing.
    /// </summary>
    public int Processing { get; set; }

    /// <summary>
    /// Gets or sets the number of scheduled jobs.
    /// </summary>
    public int Scheduled { get; set; }

    /// <summary>
    /// Gets or sets the number of failed jobs.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Gets or sets the number of succeeded jobs.
    /// </summary>
    public int Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the number of recurring jobs registered.
    /// </summary>
    public int Recurring { get; set; }

    /// <summary>
    /// Gets or sets the total number of jobs.
    /// </summary>
    public int Total => Pending + Processing + Scheduled + Failed + Succeeded;
}
