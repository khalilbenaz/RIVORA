namespace RVR.Framework.Webhooks.Models;

/// <summary>
/// Represents a webhook event to be published to subscribers.
/// </summary>
public class WebhookEvent
{
    /// <summary>Gets or sets the unique event identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the event type (e.g. "user.created").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when the event occurred.</summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the event data payload.</summary>
    public object Data { get; set; } = new();
}
