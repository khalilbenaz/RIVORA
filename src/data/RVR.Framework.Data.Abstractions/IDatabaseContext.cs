namespace RVR.Framework.Data.Abstractions;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Interface for database context abstraction.
/// Provides common functionality for all database contexts in the RIVORA Framework.
/// </summary>
public interface IDatabaseContext
{
    /// <summary>
    /// Gets the database provider being used.
    /// </summary>
    IDbProvider DbProvider { get; }

    /// <summary>
    /// Gets the underlying DbContext.
    /// </summary>
    DbContext DbContext { get; }

    /// <summary>
    /// Gets a DbSet for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A DbSet for the specified entity type.</returns>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
