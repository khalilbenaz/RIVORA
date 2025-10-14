using RVR.Framework.Domain.Entities;

namespace RVR.Framework.Domain.Events;

/// <summary>
/// Status of an outbox message through its processing lifecycle.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>Message is waiting to be processed.</summary>
    Pending = 0,

    /// <summary>Message is currently being processed.</summary>
    Processing = 1,

    /// <summary>Message was processed successfully.</summary>
    Processed = 2,

    /// <summary>Message processing failed but can be retried.</summary>
    Failed = 3,

    /// <summary>Message exceeded maximum retries and has been moved to dead letter.</summary>
    DeadLetter = 4
}

/// <summary>
/// Represents a message in the Outbox table for reliable event publishing.
/// </summary>
public class OutboxMessage : Entity<Guid>
{
    /// <summary>
    /// Default maximum number of retry attempts before moving to dead letter.
    /// </summary>
    public const int DefaultMaxRetries = 5;

    public OutboxMessage()
    {
        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
        Status = OutboxMessageStatus.Pending;
    }

    /// <summary>
    /// Full type name of the event (for deserialization).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Serialized event content (JSON).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Date and time the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Date and time the message was processed (null if not yet processed).
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Error message from the most recent failed processing attempt.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Current processing status of the message.
    /// </summary>
    public OutboxMessageStatus Status { get; set; }

    /// <summary>
    /// Number of times processing has been attempted.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Maximum number of retries before the message is moved to dead letter.
    /// </summary>
    public int MaxRetries { get; set; } = DefaultMaxRetries;

    /// <summary>
    /// Error message from the last failed attempt (preserved even after retry).
    /// </summary>
    public string? LastError { get; set; }
}
