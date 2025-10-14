using RVR.Framework.Billing.Models;

namespace RVR.Framework.Billing.Interfaces;

/// <summary>
/// Provides high-level billing operations for managing subscriptions, invoices, and usage.
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// Creates a Stripe Checkout session for a tenant to subscribe to a plan.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="planId">The plan identifier.</param>
    /// <param name="successUrl">The URL to redirect to after successful checkout.</param>
    /// <param name="cancelUrl">The URL to redirect to if checkout is canceled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkout session URL.</returns>
    Task<string> CreateCheckoutSessionAsync(string tenantId, Guid planId, string successUrl, string cancelUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Stripe Customer Portal session for a tenant to manage their subscription.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="returnUrl">The URL to redirect to when the portal session ends.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The portal session URL.</returns>
    Task<string> ManageSubscriptionAsync(string tenantId, string returnUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a tenant's subscription at the end of the current billing period.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CancelSubscriptionAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a tenant's subscription to a different plan.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="newPlanId">The new plan identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ChangeSubscriptionPlanAsync(string tenantId, Guid newPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current subscription for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription, or null if not found.</returns>
    Task<Subscription?> GetSubscriptionAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invoices for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of invoices.</returns>
    Task<IReadOnlyList<Invoice>> GetInvoicesAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a usage event for metered billing.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="metricName">The name of the usage metric.</param>
    /// <param name="quantity">The quantity consumed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordUsageAsync(string tenantId, string metricName, long quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a summary of usage for a tenant within a date range.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="from">The start of the date range.</param>
    /// <param name="to">The end of the date range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary of metric names to total quantities.</returns>
    Task<Dictionary<string, long>> GetUsageSummaryAsync(string tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
