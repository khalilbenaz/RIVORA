# AI Agents

**Package** : `RVR.Framework.AI.Agents`

## Description

Module d'orchestration multi-agents pour RIVORA Framework. Permet de creer des pipelines d'agents autonomes avec strategies sequentielle, parallele et ReAct (Reason + Act), ainsi que des outils (function calling) pour interagir avec des sources externes.

## Enregistrement

```csharp
builder.Services.AddRvrAIAgents(builder.Configuration);
```

## Configuration

```json
{
  "AI": {
    "Agents": {
      "Enabled": true,
      "DefaultModel": "gpt-4o",
      "MaxIterations": 10,
      "DefaultTemperature": 0.3,
      "TimeoutSeconds": 120,
      "Agents": {
        "Summary": {
          "Enabled": true,
          "Model": "gpt-4o-mini",
          "MaxTokens": 2000
        },
        "CodeReview": {
          "Enabled": true,
          "Model": "gpt-4o",
          "MaxTokens": 4000
        },
        "DataAnalyst": {
          "Enabled": true,
          "Model": "gpt-4o",
          "MaxTokens": 4000
        }
      },
      "Tools": {
        "Http": { "Enabled": true, "TimeoutSeconds": 30 },
        "Sql": { "Enabled": true, "ReadOnly": true },
        "FileRead": { "Enabled": true, "AllowedExtensions": [".txt", ".csv", ".json", ".md"] }
      }
    }
  }
}
```

## Interface IAgent

```csharp
public interface IAgent
{
    string Name { get; }
    string Description { get; }

    Task<AgentResult> ExecuteAsync(
        AgentContext context, CancellationToken ct = default);
}

public record AgentContext(
    string Input,
    Dictionary<string, object> Variables,
    IReadOnlyList<AgentResult>? PreviousResults = null);

public record AgentResult(
    string AgentName,
    string Output,
    bool Success,
    Dictionary<string, object>? Metadata = null);
```

## AgentPipeline

Le `AgentPipeline` orchestre l'execution des agents avec une API fluide :

```csharp
public class AgentPipeline
{
    public static AgentPipelineBuilder Create() => new();
}

public class AgentPipelineBuilder
{
    /// <summary>Ajouter un agent au pipeline.</summary>
    public AgentPipelineBuilder AddAgent<T>() where T : IAgent;

    /// <summary>Strategie d'execution sequentielle (chaque agent recoit le resultat du precedent).</summary>
    public AgentPipelineBuilder Sequential();

    /// <summary>Strategie d'execution parallele (tous les agents s'executent en meme temps).</summary>
    public AgentPipelineBuilder Parallel();

    /// <summary>Strategie ReAct (boucle Reason + Act avec outils).</summary>
    public AgentPipelineBuilder ReAct(int maxIterations = 10);

    /// <summary>Ajouter un outil disponible pour les agents.</summary>
    public AgentPipelineBuilder WithTool<T>() where T : ITool;

    /// <summary>Construire le pipeline.</summary>
    public AgentPipeline Build(IServiceProvider services);
}
```

## Interface ITool

```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    JsonSchema ParametersSchema { get; }

    Task<ToolResult> ExecuteAsync(
        JsonElement parameters, CancellationToken ct = default);
}

public record ToolResult(string Output, bool Success, string? Error = null);
```

## Agents pre-construits

### SummaryAgent

Resume un texte ou un ensemble de documents :

```csharp
var pipeline = AgentPipeline.Create()
    .AddAgent<SummaryAgent>()
    .Sequential()
    .Build(serviceProvider);

var result = await pipeline.ExecuteAsync(new AgentContext(
    Input: "Texte long a resumer...",
    Variables: new() { ["maxLength"] = 200 }));

Console.WriteLine(result.First().Output);
```

### CodeReviewAgent

Analyse du code source et genere des recommandations :

```csharp
var pipeline = AgentPipeline.Create()
    .AddAgent<CodeReviewAgent>()
    .WithTool<FileReadTool>()
    .ReAct(maxIterations: 5)
    .Build(serviceProvider);

var result = await pipeline.ExecuteAsync(new AgentContext(
    Input: "Revue le fichier OrderService.cs",
    Variables: new() { ["filePath"] = "src/Services/OrderService.cs" }));
```

### DataAnalystAgent

Analyse des donnees et genere des insights :

```csharp
var pipeline = AgentPipeline.Create()
    .AddAgent<DataAnalystAgent>()
    .WithTool<SqlTool>()
    .ReAct(maxIterations: 8)
    .Build(serviceProvider);

var result = await pipeline.ExecuteAsync(new AgentContext(
    Input: "Analyse les ventes du dernier trimestre et identifie les tendances",
    Variables: new()));
```

## Outils integres

| Outil | Description | Securite |
|-------|-------------|----------|
| `HttpTool` | Appels HTTP GET/POST vers des APIs externes | Timeout configurable |
| `SqlTool` | Execution de requetes SQL | Lecture seule uniquement |
| `FileReadTool` | Lecture de fichiers locaux | Extensions autorisees configurable |

## Pipeline multi-agents

### Execution sequentielle

```csharp
var pipeline = AgentPipeline.Create()
    .AddAgent<DataAnalystAgent>()
    .AddAgent<SummaryAgent>()
    .Sequential()
    .Build(serviceProvider);

// DataAnalystAgent analyse les donnees, puis SummaryAgent resume le resultat
var results = await pipeline.ExecuteAsync(new AgentContext(
    Input: "Donnees de ventes Q4 2025",
    Variables: new()));
```

### Execution parallele

```csharp
var pipeline = AgentPipeline.Create()
    .AddAgent<CodeReviewAgent>()
    .AddAgent<SummaryAgent>()
    .Parallel()
    .Build(serviceProvider);

// Les deux agents s'executent en parallele
var results = await pipeline.ExecuteAsync(new AgentContext(
    Input: contenuFichier,
    Variables: new()));
```

### Strategie ReAct

La strategie ReAct (Reason + Act) permet a un agent de raisonner, choisir un outil, observer le resultat et iterer :

```csharp
var pipeline = AgentPipeline.Create()
    .AddAgent<DataAnalystAgent>()
    .WithTool<SqlTool>()
    .WithTool<HttpTool>()
    .ReAct(maxIterations: 10)
    .Build(serviceProvider);

// L'agent raisonne, execute des requetes SQL, appelle des APIs et synthetise
var results = await pipeline.ExecuteAsync(new AgentContext(
    Input: "Compare nos ventes avec les tendances du marche",
    Variables: new()));
```

## Creer un agent personnalise

```csharp
public class TranslationAgent : IAgent
{
    public string Name => "TranslationAgent";
    public string Description => "Traduit du texte entre langues";

    private readonly IChatClient _chatClient;

    public TranslationAgent(IChatClient chatClient)
        => _chatClient = chatClient;

    public async Task<AgentResult> ExecuteAsync(
        AgentContext context, CancellationToken ct)
    {
        var targetLang = context.Variables.GetValueOrDefault("targetLanguage", "fr");
        var request = new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = context.Input }
            },
            SystemPrompt = $"Traduis le texte suivant en {targetLang}. Renvoie uniquement la traduction."
        };

        var response = await _chatClient.ChatAsync(request, ct);
        return new AgentResult(Name, response.Content, true);
    }
}
```

## Fonctionnalites cles

- **Multi-strategies** : Sequentielle, parallele et ReAct dans un meme pipeline
- **Function calling** : Interface `ITool` pour connecter les agents a des sources externes
- **Agents pre-construits** : Summary, CodeReview et DataAnalyst prets a l'emploi
- **API fluide** : Construction declarative des pipelines
- **Iteration controlee** : Nombre maximal d'iterations configurable pour ReAct
- **Securite** : SqlTool en lecture seule, FileReadTool avec extensions filtrees
- **Extensible** : Creez vos propres agents et outils en implementant les interfaces
