namespace RVR.Framework.Messaging.Models;

/// <summary>
/// Envelope wrapping a message with metadata.
/// </summary>
public sealed class MessageEnvelope
{
    /// <summary>
    /// Unique message identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The message type name.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The serialized message data.
    /// </summary>
    public required object Data { get; init; }

    /// <summary>
    /// When the message was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional correlation identifier for tracing related messages.
    /// </summary>
    public string? CorrelationId { get; init; }
}
