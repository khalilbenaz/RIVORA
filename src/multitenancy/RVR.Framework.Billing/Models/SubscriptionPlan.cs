namespace RVR.Framework.Billing.Models;

/// <summary>
/// Represents a subscription plan available for tenants.
/// </summary>
public sealed class SubscriptionPlan
{
    /// <summary>
    /// Gets or sets the unique identifier for the plan.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the internal name of the plan (e.g., "pro", "enterprise").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name shown to users.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monthly price in the smallest currency unit (e.g., cents).
    /// </summary>
    public long MonthlyPrice { get; set; }

    /// <summary>
    /// Gets or sets the annual price in the smallest currency unit (e.g., cents).
    /// </summary>
    public long AnnualPrice { get; set; }

    /// <summary>
    /// Gets or sets the currency code (e.g., "usd", "eur").
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Gets or sets the features included in this plan.
    /// Keys are feature identifiers, values are feature descriptions or limits.
    /// </summary>
    public Dictionary<string, string> Features { get; set; } = [];

    /// <summary>
    /// Gets or sets the usage quotas for this plan.
    /// Keys are metric names, values are maximum allowed quantities.
    /// </summary>
    public Dictionary<string, long> UsageQuotas { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of trial days for this plan.
    /// </summary>
    public int TrialDays { get; set; }

    /// <summary>
    /// Gets or sets whether this plan is currently active and available for new subscriptions.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
