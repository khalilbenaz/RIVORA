namespace RVR.Framework.Billing.Models;

/// <summary>
/// Represents an invoice for a subscription billing cycle.
/// </summary>
public sealed class Invoice
{
    /// <summary>
    /// Gets or sets the unique identifier for the invoice.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the tenant identifier that owns this invoice.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription identifier this invoice belongs to.
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the Stripe invoice identifier.
    /// </summary>
    public string StripeInvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice amount in the smallest currency unit (e.g., cents).
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code (e.g., "usd", "eur").
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Gets or sets the current status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    /// <summary>
    /// Gets or sets the date when the invoice was paid, if applicable.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Gets or sets the due date for the invoice.
    /// </summary>
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Represents the possible states of an invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>The invoice is a draft and has not been finalized.</summary>
    Draft,

    /// <summary>The invoice is open and awaiting payment.</summary>
    Open,

    /// <summary>The invoice has been paid.</summary>
    Paid,

    /// <summary>The invoice has been voided.</summary>
    Void,

    /// <summary>The invoice is uncollectible.</summary>
    Uncollectible
}
