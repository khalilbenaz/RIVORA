# AI Guardrails

**Package** : `RVR.Framework.AI.Guardrails`

## Description

Middleware de securite et de validation pour les appels LLM. Permet d'intercepter les requetes avant envoi et les reponses apres reception, avec des guardrails chainables pour la detection d'injection, le masquage PII, la moderation de contenu, le controle de budget tokens et la validation de schema.

## Enregistrement

```csharp
builder.Services.AddRvrAIGuardrails(builder.Configuration);
```

## Configuration

```json
{
  "AI": {
    "Guardrails": {
      "Enabled": true,
      "PromptInjection": {
        "Enabled": true,
        "Sensitivity": "High",
        "BlockOnDetection": true,
        "CustomPatterns": [
          "ignore previous instructions",
          "oublie tes instructions"
        ]
      },
      "PiiDetection": {
        "Enabled": true,
        "MaskCharacter": "*",
        "DetectableTypes": ["Email", "Phone", "CreditCard", "SSN", "IBAN"]
      },
      "ContentModeration": {
        "Enabled": true,
        "BlockedCategories": ["Violence", "Hate", "Sexual"],
        "Threshold": 0.8
      },
      "TokenBudget": {
        "Enabled": true,
        "MaxInputTokens": 4000,
        "MaxOutputTokens": 2000,
        "MaxTotalTokensPerMinute": 50000
      },
      "OutputSchema": {
        "Enabled": false,
        "StrictValidation": true
      }
    }
  }
}
```

## Interface IGuardrail

```csharp
public interface IGuardrail
{
    string Name { get; }
    int Order { get; }

    /// <summary>Executee avant l'envoi de la requete au LLM.</summary>
    Task<GuardrailResult> BeforeRequestAsync(
        ChatRequest request, CancellationToken ct = default);

    /// <summary>Executee apres reception de la reponse du LLM.</summary>
    Task<GuardrailResult> AfterResponseAsync(
        ChatRequest request, ChatResponse response, CancellationToken ct = default);
}

public record GuardrailResult(
    bool Allowed,
    string? Reason = null,
    ChatRequest? ModifiedRequest = null,
    ChatResponse? ModifiedResponse = null);
```

## Guardrails integres

| Guardrail | Phase | Description |
|-----------|-------|-------------|
| `PromptInjectionGuardrail` | BeforeRequest | Detecte les tentatives d'injection de prompt |
| `PiiDetectionGuardrail` | BeforeRequest + AfterResponse | Masque les donnees personnelles (email, telephone, CB, etc.) |
| `ContentModerationGuardrail` | AfterResponse | Filtre le contenu inapproprie dans les reponses |
| `TokenBudgetGuardrail` | BeforeRequest | Limite le nombre de tokens par requete et par minute |
| `OutputSchemaGuardrail` | AfterResponse | Valide que la reponse respecte un schema JSON attendu |

## GuardrailPipeline

Le pipeline chaine les guardrails dans l'ordre defini par la propriete `Order` :

```csharp
public class GuardrailPipeline
{
    public GuardrailPipeline Add<T>() where T : IGuardrail;
    public Task<GuardrailResult> ExecuteBeforeAsync(ChatRequest request, CancellationToken ct);
    public Task<GuardrailResult> ExecuteAfterAsync(ChatRequest request, ChatResponse response, CancellationToken ct);
}
```

## Utilisation

### Configuration du pipeline

```csharp
builder.Services.AddRvrAIGuardrails(builder.Configuration);

// Ajout personnalise de guardrails
builder.Services.AddSingleton<IGuardrail, PromptInjectionGuardrail>();
builder.Services.AddSingleton<IGuardrail, PiiDetectionGuardrail>();
builder.Services.AddSingleton<IGuardrail, ContentModerationGuardrail>();
builder.Services.AddSingleton<IGuardrail, TokenBudgetGuardrail>();
```

### Detection d'injection de prompt

```csharp
public class SecureChatService
{
    private readonly IChatClient _chatClient;
    private readonly GuardrailPipeline _pipeline;

    public SecureChatService(IChatClient chatClient, GuardrailPipeline pipeline)
    {
        _chatClient = chatClient;
        _pipeline = pipeline;
    }

    public async Task<string> ChatAsync(string userMessage, CancellationToken ct)
    {
        var request = new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = userMessage }
            }
        };

        // Verification avant envoi
        var beforeResult = await _pipeline.ExecuteBeforeAsync(request, ct);
        if (!beforeResult.Allowed)
            return $"Requete bloquee : {beforeResult.Reason}";

        var effectiveRequest = beforeResult.ModifiedRequest ?? request;
        var response = await _chatClient.ChatAsync(effectiveRequest, ct);

        // Verification apres reception
        var afterResult = await _pipeline.ExecuteAfterAsync(effectiveRequest, response, ct);
        if (!afterResult.Allowed)
            return "La reponse a ete filtree pour des raisons de securite.";

        var effectiveResponse = afterResult.ModifiedResponse ?? response;
        return effectiveResponse.Content;
    }
}
```

### Masquage PII dans les reponses

```csharp
// Avant guardrail : "Contactez jean.dupont@email.com ou 06 12 34 56 78"
// Apres guardrail : "Contactez ****@*****.*** ou ** ** ** ** **"

var afterResult = await _pipeline.ExecuteAfterAsync(request, response, ct);
var safeContent = afterResult.ModifiedResponse?.Content ?? response.Content;
```

### Guardrail personnalise

```csharp
public class LanguageGuardrail : IGuardrail
{
    public string Name => "LanguageFilter";
    public int Order => 50;

    public Task<GuardrailResult> BeforeRequestAsync(
        ChatRequest request, CancellationToken ct)
    {
        // Forcer la reponse en francais
        request.SystemPrompt += "\nReponds toujours en francais.";
        return Task.FromResult(new GuardrailResult(true, ModifiedRequest: request));
    }

    public Task<GuardrailResult> AfterResponseAsync(
        ChatRequest request, ChatResponse response, CancellationToken ct)
        => Task.FromResult(new GuardrailResult(true));
}
```

## Integration avec RVR.Framework.AI

Le module Guardrails s'integre nativement avec le module [AI & NaturalQuery](/modules/ai). Lorsque les deux modules sont enregistres, le pipeline de guardrails est automatiquement applique a tous les appels `IChatClient`.

## Fonctionnalites cles

- **BeforeRequest / AfterResponse** : Interception bidirectionnelle des appels LLM
- **Guardrails chainables** : Pipeline ordonne par priorite
- **Detection d'injection** : Patterns configurable avec sensibilite ajustable
- **Masquage PII** : Email, telephone, carte bancaire, IBAN, numero de securite sociale
- **Budget tokens** : Limitation par requete et par fenetre temporelle
- **Validation de schema** : Assure la conformite structurelle des reponses JSON
- **Extensible** : Creez vos propres guardrails en implementant `IGuardrail`
