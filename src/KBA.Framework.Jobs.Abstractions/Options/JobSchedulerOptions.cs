namespace KBA.Framework.Jobs.Abstractions.Options;

/// <summary>
/// Configuration options for job scheduling with retry policies.
/// </summary>
public class JobSchedulerOptions
{
    /// <summary>
    /// Gets or sets the job provider to use ("Hangfire" or "Quartz").
    /// </summary>
    public string JobProvider { get; set; } = "Hangfire";

    /// <summary>
    /// Gets or sets the default maximum number of retry attempts for failed jobs.
    /// </summary>
    public int DefaultMaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to use exponential backoff for retries.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Gets or sets the database connection string for job persistence.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the database provider type for persistence.
    /// </summary>
    public string DbProvider { get; set; } = "SqlServer";

    /// <summary>
    /// Gets or sets the queue name for default jobs.
    /// </summary>
    public string DefaultQueue { get; set; } = "default";

    /// <summary>
    /// Gets or sets whether to enable dashboard/monitoring.
    /// </summary>
    public bool EnableDashboard { get; set; } = true;

    /// <summary>
    /// Gets or sets the dashboard path (for Hangfire).
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";

    /// <summary>
    /// Gets or sets whether to require authentication for dashboard access.
    /// </summary>
    public bool RequireDashboardAuth { get; set; } = true;

    /// <summary>
    /// Gets or sets the server name for multi-server scenarios.
    /// </summary>
    public string ServerName { get; set; } = "default";

    /// <summary>
    /// Gets or sets the number of worker threads.
    /// </summary>
    public int WorkerCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the schedule polling interval in seconds (for Quartz).
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to persist job data in the database.
    /// </summary>
    public bool EnablePersistence { get; set; } = true;

    /// <summary>
    /// Gets or sets the schema name for database tables.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets additional provider-specific settings.
    /// </summary>
    public IDictionary<string, string> ProviderSettings { get; set; } = new Dictionary<string, string>();
}
