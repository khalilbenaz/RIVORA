using RVR.Framework.Billing.Models;

namespace RVR.Framework.Billing.Interfaces;

/// <summary>
/// Persistence abstraction for billing data.
/// </summary>
public interface IBillingStore
{
    // Plans

    /// <summary>
    /// Gets a subscription plan by its identifier.
    /// </summary>
    Task<SubscriptionPlan?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active subscription plans.
    /// </summary>
    Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a subscription plan.
    /// </summary>
    Task SavePlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);

    // Subscriptions

    /// <summary>
    /// Gets the active subscription for a tenant.
    /// </summary>
    Task<Subscription?> GetSubscriptionByTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription by its Stripe subscription identifier.
    /// </summary>
    Task<Subscription?> GetSubscriptionByStripeIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a subscription.
    /// </summary>
    Task SaveSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default);

    // Invoices

    /// <summary>
    /// Gets all invoices for a tenant.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetInvoicesByTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invoice by its Stripe invoice identifier.
    /// </summary>
    Task<Invoice?> GetInvoiceByStripeIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates an invoice.
    /// </summary>
    Task SaveInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);

    // Usage

    /// <summary>
    /// Records a usage event.
    /// </summary>
    Task SaveUsageRecordAsync(UsageRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage records for a tenant within a date range.
    /// </summary>
    Task<IReadOnlyList<UsageRecord>> GetUsageRecordsAsync(string tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    // Payment Methods

    /// <summary>
    /// Gets all payment methods for a tenant.
    /// </summary>
    Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsByTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a payment method.
    /// </summary>
    Task SavePaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);

    // Events

    /// <summary>
    /// Saves a billing event.
    /// </summary>
    Task SaveBillingEventAsync(BillingEvent billingEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a billing event has already been processed (idempotency).
    /// </summary>
    Task<bool> EventExistsAsync(string stripeEventId, CancellationToken cancellationToken = default);

    // Customer mapping

    /// <summary>
    /// Gets the Stripe customer identifier for a tenant.
    /// </summary>
    Task<string?> GetStripeCustomerIdAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the mapping between a tenant and a Stripe customer identifier.
    /// </summary>
    Task SaveStripeCustomerIdAsync(string tenantId, string stripeCustomerId, CancellationToken cancellationToken = default);
}
