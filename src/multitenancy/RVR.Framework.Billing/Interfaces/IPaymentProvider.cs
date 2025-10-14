namespace RVR.Framework.Billing.Interfaces;

/// <summary>
/// Abstraction over payment providers (Stripe, PayPal, etc.).
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Creates a checkout session for a new subscription.
    /// </summary>
    /// <param name="customerId">The provider customer identifier.</param>
    /// <param name="priceId">The provider price identifier.</param>
    /// <param name="successUrl">The URL to redirect to after successful checkout.</param>
    /// <param name="cancelUrl">The URL to redirect to if checkout is canceled.</param>
    /// <param name="trialDays">Number of trial days, or null for no trial.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkout session URL.</returns>
    Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, int? trialDays = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a customer portal session for subscription management.
    /// </summary>
    /// <param name="customerId">The provider customer identifier.</param>
    /// <param name="returnUrl">The URL to redirect to when the portal session ends.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The portal session URL.</returns>
    Task<string> CreatePortalSessionAsync(string customerId, string returnUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a subscription at the end of the current billing period.
    /// </summary>
    /// <param name="subscriptionId">The provider subscription identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a subscription to use a different price/plan.
    /// </summary>
    /// <param name="subscriptionId">The provider subscription identifier.</param>
    /// <param name="newPriceId">The new provider price identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateSubscriptionAsync(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new customer in the payment provider.
    /// </summary>
    /// <param name="email">The customer email.</param>
    /// <param name="name">The customer name.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider customer identifier.</returns>
    Task<string> CreateCustomerAsync(string email, string name, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a subscription from the payment provider.
    /// </summary>
    /// <param name="subscriptionId">The provider subscription identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple of (status, currentPeriodStart, currentPeriodEnd, cancelAtPeriodEnd).</returns>
    Task<(string Status, DateTime PeriodStart, DateTime PeriodEnd, bool CancelAtPeriodEnd)> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
}
