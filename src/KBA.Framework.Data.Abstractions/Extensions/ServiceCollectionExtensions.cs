namespace KBA.Framework.Data.Abstractions.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring database services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds KBA database services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DatabaseOptions();
        configureOptions(options);

        // Validate options
        var validationErrors = options.Validate();
        if (validationErrors.Any())
        {
            throw new InvalidOperationException(
                $"Invalid database options: {string.Join("; ", validationErrors)}");
        }

        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds KBA database services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="sectionName">The configuration section name. Default is "Database".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Database")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new DatabaseOptions();
        var section = configuration.GetSection(sectionName);
        section.Bind(options);

        // If connection string name is provided, get the connection string
        if (!string.IsNullOrEmpty(options.ConnectionStringName))
        {
            options.ConnectionString = configuration.GetConnectionString(options.ConnectionStringName);
        }

        // Validate options
        var validationErrors = options.Validate();
        if (validationErrors.Any())
        {
            throw new InvalidOperationException(
                $"Invalid database options: {string.Join("; ", validationErrors)}");
        }

        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds KBA database services to the service collection with provider auto-detection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="configureOptions">An optional action to further configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDatabaseWithAutoDetection(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DatabaseOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new DatabaseOptions();
        var section = configuration.GetSection("Database");
        section.Bind(options);

        // Apply additional configuration if provided
        configureOptions?.Invoke(options);

        // Get connection string if not already set
        if (string.IsNullOrEmpty(options.ConnectionString) && !string.IsNullOrEmpty(options.ConnectionStringName))
        {
            options.ConnectionString = configuration.GetConnectionString(options.ConnectionStringName);
        }

        // Auto-detect provider type from connection string
        if (!string.IsNullOrEmpty(options.ConnectionString))
        {
            options.DatabaseType = DatabaseProviderFactory.DetectDatabaseType(options.ConnectionString);
        }

        // Validate options
        var validationErrors = options.Validate();
        if (validationErrors.Any())
        {
            throw new InvalidOperationException(
                $"Invalid database options: {string.Join("; ", validationErrors)}");
        }

        services.AddSingleton(options);

        return services;
    }
}
