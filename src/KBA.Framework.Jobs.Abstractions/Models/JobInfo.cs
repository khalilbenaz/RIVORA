namespace KBA.Framework.Jobs.Abstractions.Models;

using System;

/// <summary>
/// Contains information about a job execution.
/// </summary>
public class JobInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the name of the job.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the job.
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the job.
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the job started execution.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the job completed execution.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the last error message if the job failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts made.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts allowed.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets additional data associated with the job.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Gets or sets the queue name for the job.
    /// </summary>
    public string? Queue { get; set; }

    /// <summary>
    /// Gets or sets the priority of the job (lower number = higher priority).
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cron expression for recurring jobs.
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Gets or sets the interval for recurring jobs.
    /// </summary>
    public TimeSpan? RecurringInterval { get; set; }

    /// <summary>
    /// Gets or sets whether the job is recurring.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets the next scheduled execution time for recurring jobs.
    /// </summary>
    public DateTime? NextExecution { get; set; }

    /// <summary>
    /// Gets or sets the last successful execution time.
    /// </summary>
    public DateTime? LastSuccessfulExecution { get; set; }

    /// <summary>
    /// Gets or sets the total execution count.
    /// </summary>
    public long ExecutionCount { get; set; }

    /// <summary>
    /// Gets or sets the total failed execution count.
    /// </summary>
    public long FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the average execution duration in milliseconds.
    /// </summary>
    public double AverageDurationMs { get; set; }

    /// <summary>
    /// Gets or sets custom metadata for the job.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
