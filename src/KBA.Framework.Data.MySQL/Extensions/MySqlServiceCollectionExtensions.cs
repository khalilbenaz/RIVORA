namespace KBA.Framework.Data.MySQL.Extensions;

using KBA.Framework.Data.Abstractions;
using KBA.Framework.Data.MySQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring MySQL database services.
/// </summary>
public static class MySqlServiceCollectionExtensions
{
    /// <summary>
    /// Adds MySQL database provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the database options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySqlProvider(
        this IServiceCollection services,
        Action<DatabaseOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL
        };
        configureOptions(options);
        options.DatabaseType = DatabaseType.MySQL; // Ensure type is set

        services.AddSingleton<IDbProvider>(sp => new MySqlDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.MySQL, opt => new MySqlDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds MySQL database provider services to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read database options from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySqlProvider(
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
            DatabaseType = DatabaseType.MySQL,
            ConnectionString = connectionString,
            ConnectionStringName = connectionStringName
        };

        // Bind additional options from configuration section
        var section = configuration.GetSection("Database");
        section.Bind(options);

        services.AddSingleton<IDbProvider>(sp => new MySqlDbProvider(options));
        services.AddSingleton(options);

        // Register the provider factory
        DatabaseProviderFactory.RegisterProvider(DatabaseType.MySQL, opt => new MySqlDbProvider(opt));

        return services;
    }

    /// <summary>
    /// Adds MySQL DbContext to the service collection.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySqlDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<TContext>((sp, options) =>
        {
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            options.UseMySql(serverVersion, mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            });
            optionsAction?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds MySQL DbContext to the service collection using configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read the connection string from.</param>
    /// <param name="connectionStringName">The connection string name. Default is "DefaultConnection".</param>
    /// <param name="optionsAction">An optional action to configure additional options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySqlDbContext<TContext>(
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

        return services.AddMySqlDbContext<TContext>(connectionString, optionsAction);
    }
}
