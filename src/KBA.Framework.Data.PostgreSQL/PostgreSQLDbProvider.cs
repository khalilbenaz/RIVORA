namespace KBA.Framework.Data.PostgreSQL;

using System.Data;
using KBA.Framework.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

/// <summary>
/// PostgreSQL database provider implementation.
/// </summary>
public class PostgreSQLDbProvider : IDbProvider
{
    private readonly DatabaseOptions _options;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSQLDbProvider"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public PostgreSQLDbProvider(DatabaseOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connectionString = options.ConnectionString ??
            throw new InvalidOperationException("ConnectionString must be provided for PostgreSQL provider.");
    }

    /// <inheritdoc />
    public DatabaseType DatabaseType => DatabaseType.PostgreSQL;

    /// <inheritdoc />
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc />
    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    /// <inheritdoc />
    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <inheritdoc />
    public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions)
    {
        optionsBuilder.UseNpgsql(_connectionString, npgsqlOptions =>
        {
            // Configure command timeout
            npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);

            // Configure retry on failure
            if (databaseOptions.EnableRetryOnFailure)
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: databaseOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }

            // Enable migrations assembly for proper migration discovery
            npgsqlOptions.MigrationsAssembly(typeof(PostgreSQLDbProvider).Assembly.GetName().Name);
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
        // PostgreSQL specific model configurations
        // Use snake_case naming convention by default
        modelBuilder.UseSnakeCaseNamingConvention();
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
    public string GetProviderName() => "Npgsql.EntityFrameworkCore.PostgreSQL";
}
