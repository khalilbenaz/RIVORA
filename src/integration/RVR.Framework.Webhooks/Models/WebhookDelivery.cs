namespace RVR.Framework.Webhooks.Models;

/// <summary>
/// Represents a single delivery attempt for a webhook event.
/// </summary>
public class WebhookDelivery
{
    /// <summary>Gets or sets the unique identifier for this delivery attempt.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the subscription identifier this delivery belongs to.</summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>Gets or sets the event type that triggered this delivery.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized JSON payload that was sent.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP status code returned by the callback endpoint.</summary>
    public int StatusCode { get; set; }

    /// <summary>Gets or sets whether this delivery attempt was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the error message if the delivery failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets the attempt number (1-based).</summary>
    public int AttemptNumber { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this attempt was made.</summary>
    public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the duration of the HTTP request.</summary>
    public TimeSpan Duration { get; set; }
}
