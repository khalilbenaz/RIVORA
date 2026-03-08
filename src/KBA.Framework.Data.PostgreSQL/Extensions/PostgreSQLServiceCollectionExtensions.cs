namespace KBA.Framework.Data.PostgreSQL.Extensions;

using KBA.Framework.Data.Abstractions;
using KBA.Framework.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring PostgreSQL database services.
/// </summary>
public static class PostgreSQLServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL database provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgreSQLProvider(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL
        };
        configureOptions(options);
        options.DatabaseType = DatabaseType.PostgreSQL; // Ensure type is set

        services.AddSingleton<IDbProvider>(sp => new PostgreSQLDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.PostgreSQL, opt => new PostgreSQLDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL database provider services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgreSQLProvider(
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
            DatabaseType = DatabaseType.PostgreSQL,
            ConnectionString = connectionString,
            ConnectionStringName = connectionStringName
        };

        // Bind additional options from configuration section
        var section = configuration.GetSection("Database");
        section.Bind(options);

        services.AddSingleton<IDbProvider>(sp => new PostgreSQLDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.PostgreSQL, opt => new PostgreSQLDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL DbContext to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgreSQLDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            });
            optionsAction?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL DbContext to the service collection using configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read the connection string from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgreSQLDbContext<TContext>(
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

        return services.AddPostgreSQLDbContext<TContext>(connectionString, optionsAction);
    }
}
