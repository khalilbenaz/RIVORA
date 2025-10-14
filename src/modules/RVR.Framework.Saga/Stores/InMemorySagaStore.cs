namespace RVR.Framework.Saga.Stores;

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RVR.Framework.Saga.Abstractions;
using RVR.Framework.Saga.Models;

/// <summary>
/// In-memory implementation of <see cref="ISagaStore"/> using ConcurrentDictionary.
/// Suitable for testing and development scenarios.
/// </summary>
public class InMemorySagaStore : ISagaStore
{
    private readonly ConcurrentDictionary<Guid, string> _store = new();
    private readonly ILogger<InMemorySagaStore> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="InMemorySagaStore"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public InMemorySagaStore(ILogger<InMemorySagaStore> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SaveAsync<TData>(SagaState<TData> state, CancellationToken cancellationToken = default) where TData : class, new()
    {
        var json = JsonSerializer.Serialize(state);
        _store.AddOrUpdate(state.Id, json, (_, _) => json);
        _logger.LogDebug("Saved saga state '{SagaId}' with status '{Status}'", state.Id, state.Status);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SagaState<TData>?> LoadAsync<TData>(Guid sagaId, CancellationToken cancellationToken = default) where TData : class, new()
    {
        if (_store.TryGetValue(sagaId, out var json))
        {
            var state = JsonSerializer.Deserialize<SagaState<TData>>(json);
            return Task.FromResult(state);
        }

        return Task.FromResult<SagaState<TData>?>(null);
    }

    /// <inheritdoc />
    public async Task MarkCompletedAsync<TData>(Guid sagaId, CancellationToken cancellationToken = default) where TData : class, new()
    {
        var state = await LoadAsync<TData>(sagaId, cancellationToken);
        if (state != null)
        {
            state.Status = SagaStatus.Completed;
            state.CompletedAt = DateTime.UtcNow;
            await SaveAsync(state, cancellationToken);
            _logger.LogInformation("Saga '{SagaId}' marked as completed", sagaId);
        }
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync<TData>(Guid sagaId, string errorMessage, CancellationToken cancellationToken = default) where TData : class, new()
    {
        var state = await LoadAsync<TData>(sagaId, cancellationToken);
        if (state != null)
        {
            state.Status = SagaStatus.Failed;
            state.ErrorMessage = errorMessage;
            state.CompletedAt = DateTime.UtcNow;
            await SaveAsync(state, cancellationToken);
            _logger.LogWarning("Saga '{SagaId}' marked as failed: {Error}", sagaId, errorMessage);
        }
    }
}
