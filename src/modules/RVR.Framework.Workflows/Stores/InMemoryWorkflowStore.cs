namespace RVR.Framework.Workflows.Stores;

using System.Collections.Concurrent;
using System.Text.Json;
using RVR.Framework.Workflows.Abstractions;
using RVR.Framework.Workflows.Models;

/// <summary>
/// In-memory implementation of <see cref="IWorkflowStore"/> for development and testing.
/// Data is not persisted across application restarts.
/// </summary>
public class InMemoryWorkflowStore : IWorkflowStore
{
    private readonly ConcurrentDictionary<Guid, string> _instances = new();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <inheritdoc />
    public Task SaveAsync<TState>(WorkflowInstance<TState> instance, CancellationToken cancellationToken = default)
        where TState : notnull
    {
        var json = JsonSerializer.Serialize(instance, SerializerOptions);
        _instances[instance.Id] = json;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<WorkflowInstance<TState>?> GetAsync<TState>(Guid instanceId, CancellationToken cancellationToken = default)
        where TState : notnull
    {
        if (_instances.TryGetValue(instanceId, out var json))
        {
            var instance = JsonSerializer.Deserialize<WorkflowInstance<TState>>(json, SerializerOptions);
            return Task.FromResult(instance);
        }

        return Task.FromResult<WorkflowInstance<TState>?>(null);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkflowHistoryEntry<TState>>> GetHistoryAsync<TState>(
        Guid instanceId,
        CancellationToken cancellationToken = default)
        where TState : notnull
    {
        if (_instances.TryGetValue(instanceId, out var json))
        {
            var instance = JsonSerializer.Deserialize<WorkflowInstance<TState>>(json, SerializerOptions);
            IReadOnlyList<WorkflowHistoryEntry<TState>> history = instance?.History.AsReadOnly()
                ?? (IReadOnlyList<WorkflowHistoryEntry<TState>>)[];
            return Task.FromResult(history);
        }

        return Task.FromResult<IReadOnlyList<WorkflowHistoryEntry<TState>>>([]);
    }
}
