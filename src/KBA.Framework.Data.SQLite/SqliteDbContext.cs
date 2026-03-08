namespace KBA.Framework.Data.SQLite;

using KBA.Framework.Data.Abstractions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Base class for SQLite database contexts.
/// Provides common functionality and SQLite-specific configurations.
/// </summary>
public abstract class SqliteDbContext : DbContext, IDatabaseContext
{
    private readonly IDbProvider _dbProvider;
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    protected SqliteDbContext(DbContextOptions options)
        : base(options)
    {
        _dbProvider = new SQLiteDbProvider(
            new DatabaseOptions { ConnectionString = Database.GetConnectionString() ?? string.Empty });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDbContext"/> class with a specific provider.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="dbProvider">The database provider.</param>
    protected SqliteDbContext(DbContextOptions options, IDbProvider dbProvider)
        : base(options)
    {
        _dbProvider = dbProvider;
    }

    /// <inheritdoc />
    public IDbProvider DbProvider => _dbProvider;

    /// <inheritdoc />
    DbContext IDatabaseContext.DbContext => this;

    /// <inheritdoc />
    public new DbSet<TEntity> Set<TEntity>() where TEntity : class => base.Set<TEntity>();

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _dbProvider.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Migrates the database to the latest version.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await _dbProvider.MigrateAsync(this, cancellationToken);
    }

    /// <summary>
    /// Ensures the database exists.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the database was created; false if it already existed.</returns>
    public async Task<bool> EnsureDatabaseExistsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbProvider.EnsureDatabaseExistsAsync(this, cancellationToken);
    }
}
