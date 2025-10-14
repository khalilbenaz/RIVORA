namespace RVR.Framework.Jobs.Abstractions;

/// <summary>
/// Defines the possible states of a job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// The job is waiting to be executed.
    /// </summary>
    Pending,

    /// <summary>
    /// The job is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// The job completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The job failed during execution.
    /// </summary>
    Failed,

    /// <summary>
    /// The job was cancelled before completion.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The job is scheduled but not yet queued.
    /// </summary>
    Scheduled,

    /// <summary>
    /// The job is waiting for a retry attempt.
    /// </summary>
    WaitingForRetry
}
