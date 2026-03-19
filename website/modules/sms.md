# Module SMS

**Package** : `RVR.Framework.Sms`

## Description

Module d'envoi de SMS multi-fournisseur pour RIVORA Framework. Supporte 5 providers via une abstraction unique, avec retry exponentiel et utilisation de `IHttpClientFactory` (aucun SDK fournisseur requis).

## Providers supportes

| Provider | Identifiant | Usage |
|----------|-------------|-------|
| Twilio | `Twilio` | Production |
| Vonage | `Vonage` | Production |
| OVH | `Ovh` | Production |
| Azure Communication Services | `Azure` | Production |
| Console | `Console` | Developpement / Tests |

## Enregistrement

```csharp
builder.Services.AddRvrSms(builder.Configuration);
```

Le module `SmsModule` implemente `IRvrModule` et est decouvert automatiquement.

## Configuration

```json
{
  "SMS": {
    "Provider": "Twilio",
    "DefaultFrom": "+33612345678",
    "Retry": {
      "MaxRetries": 3,
      "InitialDelayMs": 500,
      "BackoffMultiplier": 2.0
    },
    "Twilio": {
      "AccountSid": "AC...",
      "AuthToken": "your-auth-token"
    },
    "Vonage": {
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret"
    },
    "Ovh": {
      "ApplicationKey": "your-app-key",
      "ApplicationSecret": "your-app-secret",
      "ConsumerKey": "your-consumer-key",
      "ServiceName": "sms-xxxxx"
    },
    "Azure": {
      "ConnectionString": "endpoint=https://your-resource.communication.azure.com/;accesskey=..."
    }
  }
}
```

## Interface ISmsService

```csharp
public interface ISmsService
{
    /// <summary>Envoyer un SMS a un destinataire.</summary>
    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default);

    /// <summary>Envoyer un SMS a plusieurs destinataires.</summary>
    Task<IReadOnlyList<SmsResult>> SendBulkAsync(
        IEnumerable<SmsMessage> messages, CancellationToken ct = default);

    /// <summary>Recuperer le statut d'un SMS envoye.</summary>
    Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default);
}

public record SmsMessage(string To, string Body, string? From = null);

public record SmsResult(bool Success, string? MessageId, string? Error);

public enum SmsStatus { Pending, Sent, Delivered, Failed, Unknown }
```

## Utilisation

### Envoi simple

```csharp
public class NotificationService
{
    private readonly ISmsService _smsService;

    public NotificationService(ISmsService smsService)
        => _smsService = smsService;

    public async Task EnvoyerCodeVerificationAsync(string telephone, string code, CancellationToken ct)
    {
        var message = new SmsMessage(
            To: telephone,
            Body: $"Votre code de verification RIVORA : {code}");

        var result = await _smsService.SendAsync(message, ct);

        if (!result.Success)
            throw new InvalidOperationException($"Echec d'envoi SMS : {result.Error}");
    }
}
```

### Envoi en masse

```csharp
public async Task EnvoyerPromoAsync(List<string> numeros, string promo, CancellationToken ct)
{
    var messages = numeros.Select(n => new SmsMessage(
        To: n,
        Body: $"Offre speciale : {promo}. Repondez STOP pour vous desabonner."));

    var results = await _smsService.SendBulkAsync(messages, ct);

    var echecs = results.Where(r => !r.Success).ToList();
    if (echecs.Any())
        _logger.LogWarning("{Count} SMS en echec sur {Total}", echecs.Count, results.Count);
}
```

### Verification de statut

```csharp
public async Task<SmsStatus> VerifierStatutAsync(string messageId, CancellationToken ct)
{
    return await _smsService.GetStatusAsync(messageId, ct);
}
```

## Fonctionnalites cles

- **Multi-provider** : Changez de fournisseur sans modifier le code applicatif
- **Retry exponentiel** : Relance automatique avec backoff configurable en cas d'echec transitoire
- **IHttpClientFactory** : Gestion optimisee des connexions HTTP, aucun SDK fournisseur necessaire
- **Console provider** : Affiche les SMS dans la console pour le developpement local
- **Envoi en masse** : Methode `SendBulkAsync` pour les campagnes et notifications groupees
- **Suivi de statut** : Recuperation du statut de livraison via `GetStatusAsync`

Voir le [guide Installation](/guide/installation) pour la configuration globale du framework.
