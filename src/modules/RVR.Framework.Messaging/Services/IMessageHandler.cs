namespace RVR.Framework.Messaging.Services;

/// <summary>
/// Handles messages of a specific type.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public interface IMessageHandler<in T> where T : class
{
    /// <summary>
    /// Handles the received message.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    Task HandleAsync(T message, CancellationToken ct = default);
}
