namespace RVR.Framework.HealthChecks.Checks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check for background job processing systems.
/// </summary>
public class JobsHealthCheck : IHealthCheck
{
    private readonly IJobHealthMonitor _jobMonitor;
    private readonly IEnumerable<string> _criticalQueues;
    private readonly int _maxStaleJobs;
    private readonly TimeSpan _maxJobAge;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobsHealthCheck"/> class.
    /// </summary>
    /// <param name="jobMonitor">The job health monitor.</param>
    /// <param name="criticalQueues">The list of critical queue names to monitor.</param>
    /// <param name="maxStaleJobs">Maximum number of stale jobs before reporting unhealthy.</param>
    /// <param name="maxJobAge">Maximum age for jobs before reporting unhealthy.</param>
    public JobsHealthCheck(
        IJobHealthMonitor jobMonitor,
        IEnumerable<string>? criticalQueues = null,
        int maxStaleJobs = 100,
        TimeSpan? maxJobAge = null)
    {
        _jobMonitor = jobMonitor ?? throw new ArgumentNullException(nameof(jobMonitor));
        _criticalQueues = criticalQueues ?? Enumerable.Empty<string>();
        _maxStaleJobs = maxStaleJobs;
        _maxJobAge = maxJobAge ?? TimeSpan.FromHours(24);
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jobStats = await _jobMonitor.GetJobStatisticsAsync(cancellationToken);
            var issues = new List<string>();
            var data = new Dictionary<string, object>
            {
                ["TotalJobs"] = jobStats.TotalJobs,
                ["PendingJobs"] = jobStats.PendingJobs,
                ["ProcessingJobs"] = jobStats.ProcessingJobs,
                ["FailedJobs"] = jobStats.FailedJobs,
                ["CompletedJobs"] = jobStats.CompletedJobs
            };

            // Check for stale jobs
            if (jobStats.StaleJobs > _maxStaleJobs)
            {
                issues.Add($"Too many stale jobs: {jobStats.StaleJobs} (max: {_maxStaleJobs})");
            }

            data["StaleJobs"] = jobStats.StaleJobs;

            // Check for oldest job age
            if (jobStats.OldestJobAge > _maxJobAge)
            {
                issues.Add($"Oldest job exceeds maximum age: {jobStats.OldestJobAge} (max: {_maxJobAge})");
            }

            data["OldestJobAge"] = jobStats.OldestJobAge;

            // Check critical queues
            foreach (var queue in _criticalQueues)
            {
                var queueStats = await _jobMonitor.GetQueueStatisticsAsync(queue, cancellationToken);
                data[$"Queue_{queue}"] = queueStats;

                if (queueStats.IsBlocked)
                {
                    issues.Add($"Critical queue '{queue}' is blocked");
                }
            }

            // Check failure rate
            var failureRate = jobStats.TotalJobs > 0
                ? (double)jobStats.FailedJobs / jobStats.TotalJobs
                : 0;

            if (failureRate > 0.1) // More than 10% failure rate
            {
                issues.Add($"High job failure rate: {failureRate:P1}");
            }

            data["FailureRate"] = failureRate;

            if (issues.Count == 0)
            {
                return HealthCheckResult.Healthy(
                    "Job processing system is healthy",
                    data);
            }

            if (issues.Count <= 2)
            {
                return HealthCheckResult.Degraded(
                    $"Job processing system has issues: {string.Join("; ", issues)}",
                    null,
                    data);
            }

            return HealthCheckResult.Unhealthy(
                $"Job processing system is unhealthy: {string.Join("; ", issues)}",
                null,
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Job health check failed",
                ex);
        }
    }
}

/// <summary>
/// Interface for monitoring job processing health.
/// </summary>
public interface IJobHealthMonitor
{
    /// <summary>
    /// Gets statistics about job processing.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Job statistics.</returns>
    Task<JobStatistics> GetJobStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics for a specific queue.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Queue statistics.</returns>
    Task<QueueStatistics> GetQueueStatisticsAsync(string queueName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents job processing statistics.
/// </summary>
public class JobStatistics
{
    /// <summary>
    /// Gets or sets the total number of jobs.
    /// </summary>
    public int TotalJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of pending jobs.
    /// </summary>
    public int PendingJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of jobs currently processing.
    /// </summary>
    public int ProcessingJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of failed jobs.
    /// </summary>
    public int FailedJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of completed jobs.
    /// </summary>
    public int CompletedJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of stale jobs.
    /// </summary>
    public int StaleJobs { get; set; }

    /// <summary>
    /// Gets or sets the age of the oldest job.
    /// </summary>
    public TimeSpan OldestJobAge { get; set; }
}

/// <summary>
/// Represents queue statistics.
/// </summary>
public class QueueStatistics
{
    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of jobs in the queue.
    /// </summary>
    public int QueueLength { get; set; }

    /// <summary>
    /// Gets or sets whether the queue is blocked.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Gets or sets the number of workers processing this queue.
    /// </summary>
    public int WorkerCount { get; set; }

    /// <summary>
    /// Gets or sets the average processing time.
    /// </summary>
    public TimeSpan AverageProcessingTime { get; set; }
}

/// <summary>
/// In-memory implementation of IJobHealthMonitor for testing.
/// </summary>
public class InMemoryJobHealthMonitor : IJobHealthMonitor
{
    private readonly Func<Task<JobStatistics>> _getStatsFunc;
    private readonly Func<string, Task<QueueStatistics>> _getQueueStatsFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryJobHealthMonitor"/> class.
    /// </summary>
    /// <param name="getStatsFunc">Function to get job statistics.</param>
    /// <param name="getQueueStatsFunc">Function to get queue statistics.</param>
    public InMemoryJobHealthMonitor(
        Func<Task<JobStatistics>>? getStatsFunc = null,
        Func<string, Task<QueueStatistics>>? getQueueStatsFunc = null)
    {
        _getStatsFunc = getStatsFunc ?? (() => Task.FromResult(new JobStatistics()));
        _getQueueStatsFunc = getQueueStatsFunc ?? (_ => Task.FromResult(new QueueStatistics()));
    }

    /// <inheritdoc/>
    public Task<JobStatistics> GetJobStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return _getStatsFunc();
    }

    /// <inheritdoc/>
    public Task<QueueStatistics> GetQueueStatisticsAsync(string queueName, CancellationToken cancellationToken = default)
    {
        return _getQueueStatsFunc(queueName);
    }
}
