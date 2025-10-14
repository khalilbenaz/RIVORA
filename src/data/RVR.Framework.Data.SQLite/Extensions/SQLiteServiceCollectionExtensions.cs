namespace RVR.Framework.Data.SQLite.Extensions;

using RVR.Framework.Data.Abstractions;
using RVR.Framework.Data.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring SQLite database services.
/// </summary>
public static class SQLiteServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQLite database provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSQLiteProvider(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite
        };
        configureOptions(options);
        options.DatabaseType = DatabaseType.SQLite; // Ensure type is set

        services.AddSingleton<IDbProvider>(sp => new SQLiteDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.SQLite, opt => new SQLiteDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds SQLite database provider services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSQLiteProvider(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' not found in configuration.");
        }

        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite,
            ConnectionString = connectionString,
            ConnectionStringName = connectionStringName
        };

        // Bind additional options from configuration section
        var section = configuration.GetSection("Database");
        section.Bind(options);

        services.AddSingleton<IDbProvider>(sp => new SQLiteDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.SQLite, opt => new SQLiteDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds SQLite DbContext to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSQLiteDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);
            optionsAction?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds SQLite DbContext to the service collection using configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read the connection string from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSQLiteDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' not found in configuration.");
        }

        return services.AddSQLiteDbContext<TContext>(connectionString, optionsAction);
    }
}
