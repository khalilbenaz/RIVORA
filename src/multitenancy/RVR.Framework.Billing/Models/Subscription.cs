namespace RVR.Framework.Billing.Models;

/// <summary>
/// Represents a tenant's subscription to a plan.
/// </summary>
public sealed class Subscription
{
    /// <summary>
    /// Gets or sets the unique identifier for the subscription.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier that owns this subscription.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan identifier this subscription is associated with.
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Gets or sets the Stripe subscription identifier.
    /// </summary>
    public string StripeSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the subscription.
    /// </summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;

    /// <summary>
    /// Gets or sets the start of the current billing period.
    /// </summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>
    /// Gets or sets the end of the current billing period.
    /// </summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets whether the subscription will be canceled at the end of the current period.
    /// </summary>
    public bool CancelAtPeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets the date when the subscription was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents the possible states of a subscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>The subscription is in a trial period.</summary>
    Trialing,

    /// <summary>The subscription is active and in good standing.</summary>
    Active,

    /// <summary>Payment has failed but the subscription is not yet canceled.</summary>
    PastDue,

    /// <summary>The subscription has been canceled.</summary>
    Canceled,

    /// <summary>The subscription has been suspended due to non-payment or policy violation.</summary>
    Suspended
}
