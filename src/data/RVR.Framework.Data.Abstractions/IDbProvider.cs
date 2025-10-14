namespace RVR.Framework.Data.Abstractions;

using System.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Interface for database provider abstraction.
/// Provides methods for connection management and provider-specific operations.
/// </summary>
public interface IDbProvider
{
    /// <summary>
    /// Gets the type of database this provider supports.
    /// </summary>
    DatabaseType DatabaseType { get; }

    /// <summary>
    /// Gets the connection string for the database.
    /// </summary>
    /// <returns>The connection string.</returns>
    string GetConnectionString();

    /// <summary>
    /// Opens a database connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new database connection without opening it.
    /// </summary>
    /// <returns>A new database connection.</returns>
    IDbConnection CreateConnection();

    /// <summary>
    /// Configures the DbContext options for this provider.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    /// <param name="databaseOptions">The database options.</param>
    /// <returns>The configured options builder.</returns>
    DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions);

    /// <summary>
    /// Applies provider-specific model configurations.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    void OnModelCreating(ModelBuilder modelBuilder);

    /// <summary>
    /// Migrates the database to the latest version asynchronously.
    /// </summary>
    /// <param name="dbContext">The DbContext to migrate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MigrateAsync(DbContext dbContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the database exists asynchronously.
    /// </summary>
    /// <param name="dbContext">The DbContext to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the database was created; false if it already existed.</returns>
    Task<bool> EnsureDatabaseExistsAsync(DbContext dbContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL command asynchronously.
    /// </summary>
    /// <param name="dbContext">The DbContext to execute against.</param>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteSqlRawAsync(DbContext dbContext, string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the provider name for EF Core.
    /// </summary>
    /// <returns>The provider name.</returns>
    string GetProviderName();
}
