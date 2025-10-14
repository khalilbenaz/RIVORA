namespace RVR.Framework.Messaging.Services;

/// <summary>
/// Abstraction for publishing and subscribing to messages.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes a message to all registered handlers.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Subscribes a handler for the specified message type.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="handler">The message handler.</param>
    Task SubscribeAsync<T>(IMessageHandler<T> handler) where T : class;
}
