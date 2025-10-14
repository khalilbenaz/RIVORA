namespace RVR.Framework.Data.SqlServer;

using System.Data;
using Microsoft.Data.SqlClient;
using RVR.Framework.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// SQL Server database provider implementation.
/// </summary>
public class SqlServerDbProvider : IDbProvider
{
    private readonly DatabaseOptions _options;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDbProvider"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public SqlServerDbProvider(DatabaseOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connectionString = options.ConnectionString ??
            throw new InvalidOperationException("ConnectionString must be provided for SQL Server provider.");
    }

    /// <inheritdoc />
    public DatabaseType DatabaseType => DatabaseType.SqlServer;

    /// <inheritdoc />
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc />
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    /// <inheritdoc />
    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <inheritdoc />
    public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions)
    {
        optionsBuilder.UseSqlServer(_connectionString, sqlOptions =>
        {
            // Configure command timeout
            sqlOptions.CommandTimeout(databaseOptions.CommandTimeout);

            // Configure retry on failure
            if (databaseOptions.EnableRetryOnFailure)
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: databaseOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }

            // Enable migrations assembly for proper migration discovery
            sqlOptions.MigrationsAssembly(typeof(SqlServerDbProvider).Assembly.GetName().Name);
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
        // SQL Server specific model configurations
        // Add any provider-specific configurations here
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
    public string GetProviderName() => "Microsoft.EntityFrameworkCore.SqlServer";
}
