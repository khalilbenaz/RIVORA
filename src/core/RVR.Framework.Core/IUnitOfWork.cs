namespace RVR.Framework.Core;

/// <summary>
/// Unit of Work interface. Coordinates the work of multiple repositories
/// by ensuring all changes are committed (or rolled back) as a single transaction.
/// Repositories should NOT call SaveChanges themselves; only the Unit of Work should.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Persists all pending changes to the underlying store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins an explicit database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken ct = default);
}
