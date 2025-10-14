# Webhooks Module

**Package**: `RVR.Framework.Webhooks`

SaaS-ready webhook system with HMAC-SHA256 signing, exponential retry backoff and admin dashboard.

```csharp
builder.Services.AddRvrWebhooks(options =>
{
    options.SigningAlgorithm = "HMAC-SHA256";
    options.MaxRetries = 5;
});
```

See the [French documentation](/modules/webhooks) for detailed API reference.
