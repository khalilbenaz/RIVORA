# Webhooks

## Vue d'ensemble

Le module Webhooks permet de notifier des systemes externes lors d'evenements applicatifs, avec signature HMAC-SHA256 et retry exponentiel.

## Configuration

```csharp
builder.Services.AddRvrWebhooks(options =>
{
    options.SigningAlgorithm = "HMAC-SHA256";
    options.MaxRetries = 5;
    options.RetryBackoffSeconds = [5, 15, 60, 300, 900];
    options.TimeoutSeconds = 30;
});
```

## Enregistrer un webhook

```bash
curl -X POST http://localhost:5220/api/v1/webhooks \
  -H "Authorization: Bearer <token>" \
  -d '{
    "url": "https://example.com/webhook",
    "events": ["order.created", "order.paid", "product.updated"],
    "secret": "whsec_your_signing_secret"
  }'
```

## Publier un evenement

```csharp
public class OrderCreatedHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IWebhookPublisher _publisher;

    public async Task Handle(OrderCreatedEvent evt, CancellationToken ct)
    {
        await _publisher.PublishAsync("order.created", new
        {
            orderId = evt.OrderId,
            customerId = evt.CustomerId,
            total = evt.Total
        }, ct);
    }
}
```

## Verification de signature

Cote recepteur, verifiez le header `X-Webhook-Signature` :

```csharp
var payload = await new StreamReader(Request.Body).ReadToEndAsync();
var signature = Request.Headers["X-Webhook-Signature"].FirstOrDefault();
var isValid = WebhookValidator.Validate(payload, signature, secret);
```
