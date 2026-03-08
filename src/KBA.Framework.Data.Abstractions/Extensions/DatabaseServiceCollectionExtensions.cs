namespace KBA.Framework.Data.Abstractions.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for configuring database services with auto-migration support.
/// </summary>
public static class DatabaseServiceCollectionExtensions
{
    /// <summary>
    /// Adds KBA database services with automatic migration support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDatabaseWithMigration(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // First add the base database services
        services.AddKbaDatabase(configureOptions);

        // Add the auto-migration hosted service
        services.AddHostedService<AutoMigrationHostedService>();

        return services;
    }

    /// <summary>
    /// Adds KBA database services with automatic migration support using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="sectionName">The configuration section name. Default is "Database".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDatabaseWithMigration(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Database")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // First add the base database services
        services.AddKbaDatabase(configuration, sectionName);

        // Add the auto-migration hosted service
        services.AddHostedService<AutoMigrationHostedService>();

        return services;
    }

    /// <summary>
    /// Adds a DbContext with automatic migration support.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDbContext<TContext>(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
        where TContext : DbContext
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

        // Register the DbContext with the provider-specific configuration
        services.AddDbContext<TContext>((sp, dbContextOptions) =>
        {
            var dbOptions = sp.GetRequiredService<DatabaseOptions>();
            var provider = DatabaseProviderFactory.CreateProvider(dbOptions);
            provider.ConfigureDbContext(dbContextOptions, dbOptions);
        });

        // Add the auto-migration hosted service if AutoMigrate is enabled
        if (options.AutoMigrate)
        {
            services.AddHostedService<AutoMigrationHostedService>();
        }

        return services;
    }

    /// <summary>
    /// Adds a DbContext with automatic migration support using configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="sectionName">The configuration section name. Default is "Database".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Database")
        where TContext : DbContext
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

        // Register the DbContext with the provider-specific configuration
        services.AddDbContext<TContext>((sp, dbContextOptions) =>
        {
            var dbOptions = sp.GetRequiredService<DatabaseOptions>();
            var provider = DatabaseProviderFactory.CreateProvider(dbOptions);
            provider.ConfigureDbContext(dbContextOptions, dbOptions);
        });

        // Add the auto-migration hosted service if AutoMigrate is enabled
        if (options.AutoMigrate)
        {
            services.AddHostedService<AutoMigrationHostedService>();
        }

        return services;
    }

    /// <summary>
    /// Adds a DbContext with automatic migration support and provider auto-detection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="configureOptions">An optional action to further configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKbaDbContextWithAutoDetection<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DatabaseOptions>? configureOptions = null)
        where TContext : DbContext
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

        // Register the DbContext with the provider-specific configuration
        services.AddDbContext<TContext>((sp, dbContextOptions) =>
        {
            var dbOptions = sp.GetRequiredService<DatabaseOptions>();
            var provider = DatabaseProviderFactory.CreateProvider(dbOptions);
            provider.ConfigureDbContext(dbContextOptions, dbOptions);
        });

        // Add the auto-migration hosted service if AutoMigrate is enabled
        if (options.AutoMigrate)
        {
            services.AddHostedService<AutoMigrationHostedService>();
        }

        return services;
    }
}
