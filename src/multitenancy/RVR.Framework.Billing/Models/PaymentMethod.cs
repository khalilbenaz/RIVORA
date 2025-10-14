namespace RVR.Framework.Billing.Models;

/// <summary>
/// Represents a stored payment method for a tenant.
/// </summary>
public sealed class PaymentMethod
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment method.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier that owns this payment method.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe payment method identifier.
    /// </summary>
    public string StripePaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of payment method (e.g., "card", "bank_account").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last four digits of the card or account number.
    /// </summary>
    public string Last4 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiry month of the payment method.
    /// </summary>
    public int? ExpiryMonth { get; set; }

    /// <summary>
    /// Gets or sets the expiry year of the payment method.
    /// </summary>
    public int? ExpiryYear { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default payment method for the tenant.
    /// </summary>
    public bool IsDefault { get; set; }
}
