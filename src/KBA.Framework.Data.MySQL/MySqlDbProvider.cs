namespace KBA.Framework.Data.MySQL;

using System.Data;
using KBA.Framework.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

/// <summary>
/// MySQL database provider implementation.
/// </summary>
public class MySqlDbProvider : IDbProvider
{
    private readonly DatabaseOptions _options;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDbProvider"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public MySqlDbProvider(DatabaseOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connectionString = options.ConnectionString ??
            throw new InvalidOperationException("ConnectionString must be provided for MySQL provider.");
    }

    /// <inheritdoc />
    public DatabaseType DatabaseType => DatabaseType.MySQL;

    /// <inheritdoc />
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc />
    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

    /// <inheritdoc />
    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <inheritdoc />
    public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions)
    {
        var serverVersion = ServerVersion.AutoDetect(_connectionString);

        optionsBuilder.UseMySql(_connectionString, serverVersion, mysqlOptions =>
        {
            // Configure command timeout
            mysqlOptions.CommandTimeout(databaseOptions.CommandTimeout);

            // Configure retry on failure
            if (databaseOptions.EnableRetryOnFailure)
            {
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: databaseOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }

            // Enable migrations assembly for proper migration discovery
            mysqlOptions.MigrationsAssembly(typeof(MySqlDbProvider).Assembly.GetName().Name);
        });

        // Configure additional options
        if (databaseOptions.EnableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        if (databaseOptions.EnableDetailedErrors)
        {
            optionsBuilder.EnableDetailedErrors();
        }

        return optionsBuilder;
    }

    /// <inheritdoc />
    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MySQL specific model configurations
        // Use lower case table names by default for cross-platform compatibility
        modelBuilder.UseLowerCaseNamingConvention();
    }

    /// <inheritdoc />
    public async Task MigrateAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> EnsureDatabaseExistsAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> ExecuteSqlRawAsync(DbContext dbContext, string sql, CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <inheritdoc />
    public string GetProviderName() => "Pomelo.EntityFrameworkCore.MySql";
}
