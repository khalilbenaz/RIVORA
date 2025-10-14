namespace RVR.Framework.Billing.Models;

/// <summary>
/// Represents a usage record for metered billing.
/// </summary>
public sealed class UsageRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the usage record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier that this usage belongs to.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the usage metric (e.g., "api_calls", "storage_gb").
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity consumed.
    /// </summary>
    public long Quantity { get; set; }

    /// <summary>
    /// Gets or sets when this usage was recorded.
    /// </summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
