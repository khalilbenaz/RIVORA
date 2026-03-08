namespace KBA.Framework.Data.SqlServer.Extensions;

using KBA.Framework.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring SQL Server database services.
/// </summary>
public static class SqlServerServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server database provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerProvider(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer
        };
        configureOptions(options);
        options.DatabaseType = DatabaseType.SqlServer; // Ensure type is set

        services.AddSingleton<IDbProvider>(sp => new SqlServerDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.SqlServer, opt => new SqlServerDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds SQL Server database provider services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerProvider(
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
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = connectionString,
            ConnectionStringName = connectionStringName
        };

        // Bind additional options from configuration section
        var section = configuration.GetSection("Database");
        section.Bind(options);

        services.AddSingleton<IDbProvider>(sp => new SqlServerDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.SqlServer, opt => new SqlServerDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds SQL Server DbContext to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            });
            optionsAction?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds SQL Server DbContext to the service collection using configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read the connection string from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerDbContext<TContext>(
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

        return services.AddSqlServerDbContext<TContext>(connectionString, optionsAction);
    }
}
