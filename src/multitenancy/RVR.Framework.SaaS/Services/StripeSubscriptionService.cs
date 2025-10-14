using RVR.Framework.SaaS.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace RVR.Framework.SaaS.Services;

public class StripeSubscriptionService : ISubscriptionService
{
    public StripeSubscriptionService(string apiKey)
    {
        StripeConfiguration.ApiKey = apiKey;
    }

    public async Task<string> CreateCheckoutSessionAsync(string tenantId, string planId)
    {
        var options = new SessionCreateOptions
        {
            SuccessUrl = "https://your-domain.com/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = "https://your-domain.com/cancel",
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = planId,
                    Quantity = 1,
                },
            },
            ClientReferenceId = tenantId
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task HandleWebhooksAsync(string json, string signature)
    {
        var endpointSecret = "whsec_..."; // Typically from configuration
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, endpointSecret);

            // Handle the event
            if (stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted)
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                // Logic to suspend the tenant based on subscription.CustomerId
            }
            // Add more webhook handlers (created, updated, payment_failed)
        }
        catch (StripeException)
        {
            // Handle error
            throw;
        }
    }
}
