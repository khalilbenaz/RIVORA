namespace KBA.Framework.Data.Abstractions;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Factory for creating database providers based on configuration.
/// </summary>
public static class DatabaseProviderFactory
{
    private static readonly IDictionary<DatabaseType, Func<DatabaseOptions, IDbProvider>> ProviderFactories =
        new Dictionary<DatabaseType, Func<DatabaseOptions, IDbProvider>>();

    /// <summary>
    /// Registers a provider factory for the specified database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="factory">The factory function to create the provider.</param>
    public static void RegisterProvider(DatabaseType databaseType, Func<DatabaseOptions, IDbProvider> factory)
    {
        ProviderFactories[databaseType] = factory;
    }

    /// <summary>
    /// Creates a database provider based on the specified database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="options">The database options.</param>
    /// <returns>The created database provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not registered.</exception>
    public static IDbProvider CreateProvider(DatabaseType databaseType, DatabaseOptions options)
    {
        if (ProviderFactories.TryGetValue(databaseType, out var factory))
        {
            return factory(options);
        }

        throw new InvalidOperationException(
            $"Database provider for '{databaseType}' is not registered. " +
            $"Ensure the corresponding KBA.Framework.Data.* package is installed and the provider is registered.");
    }

    /// <summary>
    /// Creates a database provider based on the configuration.
    /// Auto-detects the provider type from the connection string if not explicitly specified.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="options">The database options.</param>
    /// <returns>The created database provider.</returns>
    public static IDbProvider CreateProvider(IConfiguration configuration, DatabaseOptions options)
    {
        // If database type is not specified, try to auto-detect from connection string
        if (options.DatabaseType == DatabaseType.SqlServer && string.IsNullOrEmpty(options.ConnectionString))
        {
            // Try to get connection string from configuration
            var connectionStringName = options.ConnectionStringName ?? "DefaultConnection";
            options.ConnectionString = configuration.GetConnectionString(connectionStringName);
        }

        // Auto-detect provider if not explicitly set
        if (!IsProviderExplicitlySet(options) && !string.IsNullOrEmpty(options.ConnectionString))
        {
            options.DatabaseType = DetectDatabaseType(options.ConnectionString);
        }

        return CreateProvider(options.DatabaseType, options);
    }

    /// <summary>
    /// Detects the database type from the connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to analyze.</param>
    /// <returns>The detected database type.</returns>
    public static DatabaseType DetectDatabaseType(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        var lowerConnectionString = connectionString.ToLowerInvariant();

        // SQL Server detection
        if (lowerConnectionString.Contains("server=") || 
            lowerConnectionString.Contains("data source=") ||
            lowerConnectionString.Contains("address=") && lowerConnectionString.Contains("mssql") ||
            lowerConnectionString.Contains("trusted_connection=") ||
            lowerConnectionString.Contains("integrated security="))
        {
            return DatabaseType.SqlServer;
        }

        // PostgreSQL detection
        if (lowerConnectionString.StartsWith("host=") ||
            lowerConnectionString.StartsWith("server=") && lowerConnectionString.Contains("port=") ||
            lowerConnectionString.Contains("username=") && lowerConnectionString.Contains("password=") && lowerConnectionString.Contains("database="))
        {
            // Additional check for PostgreSQL-specific keywords
            if (lowerConnectionString.Contains("host=") || lowerConnectionString.Contains("username="))
            {
                return DatabaseType.PostgreSQL;
            }
        }

        // MySQL detection
        if (lowerConnectionString.Contains("server=") && lowerConnectionString.Contains("uid=") ||
            lowerConnectionString.Contains("server=") && lowerConnectionString.Contains("user id="))
        {
            return DatabaseType.MySQL;
        }

        // SQLite detection
        if (lowerConnectionString.StartsWith("data source=") && (
            lowerConnectionString.Contains(".db") || 
            lowerConnectionString.Contains(".sqlite") ||
            lowerConnectionString.Contains(":memory:")))
        {
            return DatabaseType.SQLite;
        }

        // Default to SQL Server for backward compatibility
        return DatabaseType.SqlServer;
    }

    /// <summary>
    /// Determines if the provider was explicitly set in the options.
    /// </summary>
    /// <param name="options">The database options.</param>
    /// <returns>True if explicitly set; otherwise, false.</returns>
    private static bool IsProviderExplicitlySet(DatabaseOptions options)
    {
        // Check if ProviderOptions contains an explicit provider setting
        return options.ProviderOptions.ContainsKey("ExplicitProvider");
    }

    /// <summary>
    /// Gets all registered database types.
    /// </summary>
    /// <returns>An enumerable of registered database types.</returns>
    public static IEnumerable<DatabaseType> GetRegisteredProviders()
    {
        return ProviderFactories.Keys;
    }

    /// <summary>
    /// Checks if a provider is registered for the specified database type.
    /// </summary>
    /// <param name="databaseType">The database type to check.</param>
    /// <returns>True if registered; otherwise, false.</returns>
    public static bool IsProviderRegistered(DatabaseType databaseType)
    {
        return ProviderFactories.ContainsKey(databaseType);
    }
}
