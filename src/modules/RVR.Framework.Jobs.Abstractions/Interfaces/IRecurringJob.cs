namespace RVR.Framework.Jobs.Abstractions.Interfaces;

/// <summary>
/// Interface for jobs that execute on a recurring schedule.
/// </summary>
public interface IRecurringJob : IJob
{
    /// <summary>
    /// Gets the cron expression defining the recurrence schedule.
    /// </summary>
    string CronExpression { get; }

    /// <summary>
    /// Gets the timezone for the cron schedule. Defaults to UTC.
    /// </summary>
    string TimeZoneId { get; }
}
