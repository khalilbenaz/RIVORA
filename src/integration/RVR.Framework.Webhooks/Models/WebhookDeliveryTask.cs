namespace RVR.Framework.Webhooks.Models;

/// <summary>
/// Represents a queued webhook delivery task to be processed in the background.
/// </summary>
public class WebhookDeliveryTask
{
    /// <summary>Gets or sets the target subscription.</summary>
    public required WebhookSubscription Subscription { get; init; }

    /// <summary>Gets or sets the webhook event to deliver.</summary>
    public required WebhookEvent WebhookEvent { get; init; }

    /// <summary>Gets or sets the serialized JSON payload.</summary>
    public required string Payload { get; init; }
}
