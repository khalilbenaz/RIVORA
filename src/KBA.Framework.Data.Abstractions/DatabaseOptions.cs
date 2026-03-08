namespace KBA.Framework.Data.Abstractions;

/// <summary>
/// Configuration options for database connections and behavior.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Gets or sets the type of database provider to use.
    /// </summary>
    public DatabaseType DatabaseType { get; set; }

    /// <summary>
    /// Gets or sets the connection string for the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the connection string name from configuration.
    /// If set, this will be used to retrieve the connection string from IConfiguration.
    /// </summary>
    public string? ConnectionStringName { get; set; } = "DefaultConnection";

    /// <summary>
    /// Gets or sets whether to enable retry on failure.
    /// Default is true for production scenarios.
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 5.
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// Default is 30 seconds.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to enable sensitive data logging.
    /// Should be false in production environments.
    /// Default is false.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable detailed errors.
    /// Should be false in production environments.
    /// Default is false.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to automatically run migrations on startup.
    /// Default is false.
    /// </summary>
    public bool AutoMigrate { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to use lazy loading proxies.
    /// Default is false.
    /// </summary>
    public bool UseLazyLoadingProxies { get; set; } = false;

    /// <summary>
    /// Gets or sets additional provider-specific options.
    /// </summary>
    public IDictionary<string, string> ProviderOptions { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Validates the database options.
    /// </summary>
    /// <returns>A list of validation errors. Empty if valid.</returns>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionString) && string.IsNullOrWhiteSpace(ConnectionStringName))
        {
            errors.Add("Either ConnectionString or ConnectionStringName must be provided.");
        }

        if (MaxRetryCount < 0)
        {
            errors.Add("MaxRetryCount must be greater than or equal to 0.");
        }

        if (CommandTimeout <= 0)
        {
            errors.Add("CommandTimeout must be greater than 0.");
        }

        return errors;
    }
}
