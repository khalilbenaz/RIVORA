namespace KBA.Framework.Jobs.Abstractions.Interfaces;

/// <summary>
/// Interface for cron-based jobs with additional configuration options.
/// </summary>
public interface ICronJob : IRecurringJob
{
    /// <summary>
    /// Gets a description of the job for monitoring purposes.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets whether the job should be disabled.
    /// </summary>
    bool IsDisabled { get; }

    /// <summary>
    /// Gets the job queue name for prioritization.
    /// </summary>
    string QueueName { get; }
}
