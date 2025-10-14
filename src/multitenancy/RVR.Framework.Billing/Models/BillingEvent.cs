namespace RVR.Framework.Billing.Models;

/// <summary>
/// Represents a billing event received from a payment provider webhook.
/// </summary>
public sealed class BillingEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for the billing event.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier associated with this event.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of event (e.g., "invoice.paid", "subscription.updated").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe event identifier.
    /// </summary>
    public string StripeEventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw JSON payload of the event.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this event was processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}
