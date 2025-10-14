namespace RVR.Framework.Webhooks.Models;

/// <summary>
/// Represents a webhook subscription that defines where and how webhook events should be delivered.
/// </summary>
public class WebhookSubscription
{
    /// <summary>Gets or sets the unique identifier for this subscription.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the optional tenant identifier for multi-tenant scenarios.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Gets or sets the event type to subscribe to (e.g. "user.created", "order.completed").</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL that will receive the webhook HTTP POST.</summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the HMAC-SHA256 signing secret used to sign payloads.</summary>
    public string? Secret { get; set; }

    /// <summary>Gets or sets whether this subscription is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the UTC timestamp when this subscription was created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the maximum number of delivery retry attempts.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Gets or sets custom headers as "Key:Value" pairs to include in webhook requests.</summary>
    public string[]? Headers { get; set; }
}
