namespace KBA.Framework.SaaS.Interfaces;
public interface ISubscriptionService {
    Task<string> CreateCheckoutSessionAsync(string tenantId, string planId);
    Task HandleWebhooksAsync(string json, string signature);
}
