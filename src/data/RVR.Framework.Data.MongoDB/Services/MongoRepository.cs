using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace RVR.Framework.Data.MongoDB.Services;

/// <summary>
/// Generic repository for MongoDB CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class MongoRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;
    private readonly ILogger<MongoRepository<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
    /// </summary>
    /// <param name="collection">The MongoDB collection.</param>
    /// <param name="logger">The logger.</param>
    public MongoRepository(IMongoCollection<T> collection, ILogger<MongoRepository<T>> logger)
    {
        _collection = collection;
        _logger = logger;
    }

    /// <summary>
    /// Gets an item by a filter.
    /// </summary>
    /// <param name="filter">The filter definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching item, or null.</returns>
    public async Task<T?> GetAsync(FilterDefinition<T> filter, CancellationToken ct = default)
    {
        var cursor = await _collection.FindAsync(filter, cancellationToken: ct);
        return await cursor.FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Gets all items matching a filter.
    /// </summary>
    /// <param name="filter">The filter definition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of matching items.</returns>
    public async Task<List<T>> GetAllAsync(FilterDefinition<T> filter, CancellationToken ct = default)
    {
        var cursor = await _collection.FindAsync(filter, cancellationToken: ct);
        return await cursor.ToListAsync(ct);
    }

    /// <summary>
    /// Creates a new item.
    /// </summary>
    /// <param name="item">The item to create.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task CreateAsync(T item, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(item, cancellationToken: ct);
        _logger.LogDebug("Inserted item into collection {Collection}", _collection.CollectionNamespace.CollectionName);
    }

    /// <summary>
    /// Replaces an existing item.
    /// </summary>
    /// <param name="filter">The filter to find the item.</param>
    /// <param name="item">The replacement item.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task UpdateAsync(FilterDefinition<T> filter, T item, CancellationToken ct = default)
    {
        await _collection.ReplaceOneAsync(filter, item, cancellationToken: ct);
        _logger.LogDebug("Replaced item in collection {Collection}", _collection.CollectionNamespace.CollectionName);
    }

    /// <summary>
    /// Deletes an item matching a filter.
    /// </summary>
    /// <param name="filter">The filter to find the item.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteAsync(FilterDefinition<T> filter, CancellationToken ct = default)
    {
        await _collection.DeleteOneAsync(filter, ct);
        _logger.LogDebug("Deleted item from collection {Collection}", _collection.CollectionNamespace.CollectionName);
    }
}
