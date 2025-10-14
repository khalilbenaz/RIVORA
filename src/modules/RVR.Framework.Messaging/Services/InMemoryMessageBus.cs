using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Messaging.Services;

/// <summary>
/// In-memory implementation of <see cref="IMessageBus"/>.
/// </summary>
public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();
    private readonly ILogger<InMemoryMessageBus> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryMessageBus"/> class.
    /// </summary>
    public InMemoryMessageBus(ILogger<InMemoryMessageBus> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        var messageType = typeof(T);
        _logger.LogDebug("Publishing message of type {MessageType}", messageType.Name);

        if (!_handlers.TryGetValue(messageType, out var handlers))
            return;

        var typedHandlers = handlers.OfType<IMessageHandler<T>>().ToList();

        foreach (var handler in typedHandlers)
        {
            try
            {
                await handler.HandleAsync(message, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message of type {MessageType} in handler {HandlerType}",
                    messageType.Name, handler.GetType().Name);
            }
        }
    }

    /// <inheritdoc />
    public Task SubscribeAsync<T>(IMessageHandler<T> handler) where T : class
    {
        var messageType = typeof(T);
        var handlers = _handlers.GetOrAdd(messageType, _ => []);

        lock (handlers)
        {
            handlers.Add(handler);
        }

        _logger.LogDebug("Subscribed handler {HandlerType} for message type {MessageType}",
            handler.GetType().Name, messageType.Name);

        return Task.CompletedTask;
    }
}
