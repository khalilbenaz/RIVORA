namespace RVR.Framework.Core;

using System.Linq.Expressions;

/// <summary>
/// Generic repository interface for performing CRUD operations on entities.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages.</typeparam>
public interface IRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities of the specified type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of all entities.</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of entities matching the predicate.</returns>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity matching the specified predicate, or null if not found.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if any entity matches; otherwise, false.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of entities.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The total count of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The added entity with generated properties.</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the repository.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities in the repository.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
