namespace RVR.Framework.Data.ReadReplica;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Factory that creates <see cref="DbContext"/> instances configured with the
/// appropriate connection string based on whether the query is read-only or requires writes.
/// </summary>
/// <typeparam name="TContext">The type of <see cref="DbContext"/> to create.</typeparam>
public class ReadReplicaDbContextFactory<TContext> where TContext : DbContext
{
    private readonly IDbConnectionRouter _router;
    private readonly IDbContextFactory<TContext> _innerFactory;
    private readonly ILogger<ReadReplicaDbContextFactory<TContext>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadReplicaDbContextFactory{TContext}"/> class.
    /// </summary>
    /// <param name="router">The connection router that selects the right connection string.</param>
    /// <param name="innerFactory">The inner EF Core context factory.</param>
    /// <param name="logger">The logger instance.</param>
    public ReadReplicaDbContextFactory(
        IDbConnectionRouter router,
        IDbContextFactory<TContext> innerFactory,
        ILogger<ReadReplicaDbContextFactory<TContext>> logger)
    {
        _router = router;
        _innerFactory = innerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates a <see cref="DbContext"/> configured for read-only operations
    /// using one of the read replicas. The context will have change tracking disabled.
    /// </summary>
    /// <returns>A read-only configured <typeparamref name="TContext"/>.</returns>
    public TContext CreateReadContext()
    {
        var connectionString = _router.GetConnectionString(isReadOnly: true);
        _logger.LogDebug("Creating read-only DbContext.");

        var context = _innerFactory.CreateDbContext();
        context.Database.SetConnectionString(connectionString);
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        return context;
    }

    /// <summary>
    /// Creates a <see cref="DbContext"/> configured for write operations
    /// using the primary database connection.
    /// </summary>
    /// <returns>A write-capable <typeparamref name="TContext"/>.</returns>
    public TContext CreateWriteContext()
    {
        var connectionString = _router.GetConnectionString(isReadOnly: false);
        _logger.LogDebug("Creating write DbContext.");

        var context = _innerFactory.CreateDbContext();
        context.Database.SetConnectionString(connectionString);
        return context;
    }
}
