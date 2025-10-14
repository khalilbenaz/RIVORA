# Module Webhooks

**Package** : `RVR.Framework.Webhooks`

## Description

Systeme de webhooks SaaS-ready avec signature HMAC-SHA256, retry exponentiel et dashboard d'administration.

## Enregistrement

```csharp
builder.Services.AddRvrWebhooks(options =>
{
    options.SigningAlgorithm = "HMAC-SHA256";
    options.MaxRetries = 5;
    options.RetryBackoffSeconds = [5, 15, 60, 300, 900];
});
```

## Interfaces

```csharp
public interface IWebhookPublisher
{
    Task PublishAsync(string eventType, object payload, CancellationToken ct = default);
}

public interface IWebhookSubscriptionService
{
    Task<WebhookSubscription> CreateAsync(CreateWebhookRequest request);
    Task DeleteAsync(Guid subscriptionId);
    Task<List<WebhookSubscription>> GetByTenantAsync(string tenantId);
}
```

## Evenements predefined

- `order.created`, `order.paid`, `order.cancelled`
- `product.created`, `product.updated`, `product.deleted`
- `user.registered`, `user.locked`
- `tenant.created`, `tenant.plan_changed`

Voir le [guide Webhooks](/guide/webhooks) pour l'implementation detaillee.
