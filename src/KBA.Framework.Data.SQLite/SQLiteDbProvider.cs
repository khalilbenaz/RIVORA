namespace KBA.Framework.Data.SQLite;

using System.Data;
using KBA.Framework.Data.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// SQLite database provider implementation.
/// </summary>
public class SQLiteDbProvider : IDbProvider
{
    private readonly DatabaseOptions _options;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLiteDbProvider"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public SQLiteDbProvider(DatabaseOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connectionString = options.ConnectionString ??
            throw new InvalidOperationException("ConnectionString must be provided for SQLite provider.");
    }

    /// <inheritdoc />
    public DatabaseType DatabaseType => DatabaseType.SQLite;

    /// <inheritdoc />
    public string GetConnectionString() => _connectionString;

    /// <inheritdoc />
    public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    /// <inheritdoc />
    public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <inheritdoc />
    public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions)
    {
        optionsBuilder.UseSqlite(_connectionString, sqliteOptions =>
        {
            // Configure command timeout
            sqliteOptions.CommandTimeout(databaseOptions.CommandTimeout);

            // Enable migrations assembly for proper migration discovery
            sqliteOptions.MigrationsAssembly(typeof(SQLiteDbProvider).Assembly.GetName().Name);
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
        // SQLite specific model configurations
        // SQLite doesn't support some advanced features, so we configure accordingly
    }

    /// <inheritdoc />
    public async Task MigrateAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        // SQLite doesn't support MigrateAsync in the same way as other providers
        // We use EnsureCreated instead for simplicity, or apply migrations manually
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
    public string GetProviderName() => "Microsoft.EntityFrameworkCore.Sqlite";
}
