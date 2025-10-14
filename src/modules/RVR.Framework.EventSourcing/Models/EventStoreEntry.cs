namespace RVR.Framework.EventSourcing.Models;

/// <summary>
/// Represents a single entry in the event store, wrapping a serialized domain event.
/// </summary>
public class EventStoreEntry
{
    /// <summary>
    /// Gets or sets the identifier of the event stream this entry belongs to.
    /// </summary>
    public string StreamId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fully qualified type name of the event.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized event data (JSON).
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version number of this event within the stream.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this event was stored.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
