using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Data.CosmosDB.Services;

/// <summary>
/// Generic repository for Azure Cosmos DB CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class CosmosRepository<T> where T : class
{
    private readonly Container _container;
    private readonly ILogger<CosmosRepository<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosRepository{T}"/> class.
    /// </summary>
    /// <param name="container">The Cosmos DB container.</param>
    /// <param name="logger">The logger.</param>
    public CosmosRepository(Container container, ILogger<CosmosRepository<T>> logger)
    {
        _container = container;
        _logger = logger;
    }

    /// <summary>
    /// Gets an item by its identifier and partition key.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The item, or null if not found.</returns>
    public async Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Item {Id} not found in container {Container}", id, _container.Id);
            return null;
        }
    }

    /// <summary>
    /// Creates a new item.
    /// </summary>
    /// <param name="item">The item to create.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created item.</returns>
    public async Task<T> CreateAsync(T item, string partitionKey, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey), cancellationToken: ct);
        _logger.LogDebug("Created item in container {Container}", _container.Id);
        return response.Resource;
    }

    /// <summary>
    /// Updates (upserts) an item.
    /// </summary>
    /// <param name="item">The item to upsert.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The upserted item.</returns>
    public async Task<T> UpsertAsync(T item, string partitionKey, CancellationToken ct = default)
    {
        var response = await _container.UpsertItemAsync(item, new PartitionKey(partitionKey), cancellationToken: ct);
        _logger.LogDebug("Upserted item in container {Container}", _container.Id);
        return response.Resource;
    }

    /// <summary>
    /// Deletes an item by its identifier and partition key.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: ct);
        _logger.LogDebug("Deleted item {Id} from container {Container}", id, _container.Id);
    }
}
