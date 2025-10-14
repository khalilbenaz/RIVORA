# AI & NaturalQuery

The RIVORA Framework integrates AI capabilities through two modules: a RAG (Retrieval-Augmented Generation) pipeline for knowledge-base Q&A, and NaturalQuery for querying entities using natural language in French and English.

## IChatClient

The `IChatClient` abstraction supports multiple LLM providers:

```csharp
public interface IChatClient
{
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default);
    Task<IAsyncEnumerable<string>> StreamChatAsync(ChatRequest request, CancellationToken ct = default);
}
```

### Using the chat client

```csharp
public class AssistantService
{
    private readonly IChatClient _chatClient;

    public async Task<string> AskAsync(string question, CancellationToken ct)
    {
        var request = new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = question }
            },
            Temperature = 0.7f,
            MaxTokens = 1024,
            SystemPrompt = "You are a helpful assistant for the RIVORA application."
        };

        var response = await _chatClient.ChatAsync(request, ct);
        return response.Content;
    }

    // Streaming response
    public async IAsyncEnumerable<string> StreamAskAsync(string question,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var request = new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = question }
            }
        };

        var stream = await _chatClient.StreamChatAsync(request, ct);
        await foreach (var chunk in stream.WithCancellation(ct))
        {
            yield return chunk;
        }
    }
}
```

## IVectorStore

The vector store manages embeddings for semantic search:

```csharp
public interface IVectorStore
{
    Task UpsertAsync(string id, float[] embedding, Dictionary<string, string> metadata, CancellationToken ct = default);
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
```

Built-in implementations:
- `InMemoryVectorStore` -- for development and testing
- `QdrantVectorStore` -- for production (via the samples/ai-rag-app)

## RAG Pipeline

The `IRagService` orchestrates the full RAG flow: ingest, embed, store, retrieve, and generate.

### Document Ingestion

```csharp
public class DocumentController : ControllerBase
{
    private readonly IRagService _ragService;

    [HttpPost("ingest")]
    public async Task<IActionResult> IngestDocument(
        [FromBody] IngestDocumentDto dto, CancellationToken ct)
    {
        await _ragService.IngestDocumentAsync(
            documentId: dto.DocumentId,
            content: dto.Content,
            metadata: new Dictionary<string, string>
            {
                ["source"] = dto.Source,
                ["author"] = dto.Author,
                ["category"] = dto.Category
            },
            ct: ct);

        return Accepted();
    }
}
```

### Querying the Knowledge Base

```csharp
[HttpPost("ask")]
public async Task<IActionResult> Ask([FromBody] AskDto dto, CancellationToken ct)
{
    var response = await _ragService.QueryAsync(dto.Question, new RagOptions
    {
        TopK = 5,
        Temperature = 0.3f,
        SystemPrompt = "Answer based on the provided context. Cite your sources."
    }, ct);

    return Ok(new
    {
        answer = response.Answer,
        sources = response.Sources.Select(s => new
        {
            s.DocumentId,
            s.Score,
            s.Content
        }),
        tokensUsed = response.RawResponse?.TotalTokens
    });
}
```

### RAG Pipeline Internals

```
[Document] --> Chunker --> Embedding Service --> Vector Store
                                                      |
[Question] --> Embedding Service -----------------> Search
                                                      |
                                                  Top K Results
                                                      |
                                               Context Builder
                                                      |
                                                 Chat Client --> [Answer]
```

## NaturalQuery

Query your entities using natural language in French or English. The `NaturalQueryService` parses the query, builds a `QueryPlan`, and applies it to `IQueryable<T>`.

### INaturalQueryService

```csharp
public interface INaturalQueryService
{
    QueryPlan Parse(string naturalLanguageQuery, Type entityType);
    IQueryable<T> Apply<T>(IQueryable<T> source, QueryPlan plan) where T : class;
    IQueryable<T> Query<T>(IQueryable<T> source, string naturalLanguageQuery) where T : class;
}
```

### French Examples

```csharp
// "produits actifs dont le prix est superieur a 20"
var products = _naturalQuery.Query(dbContext.Products,
    "produits actifs dont le prix est superieur a 20");

// "utilisateurs crees cette semaine"
var users = _naturalQuery.Query(dbContext.Users,
    "utilisateurs crees cette semaine");

// "commandes en attente triees par date decroissante"
var orders = _naturalQuery.Query(dbContext.Orders,
    "commandes en attente triees par date decroissante");
```

### English Examples

```csharp
// "active products with price greater than 20"
var products = _naturalQuery.Query(dbContext.Products,
    "active products with price greater than 20");

// "users created this week"
var users = _naturalQuery.Query(dbContext.Users,
    "users created this week");

// "pending orders sorted by date descending"
var orders = _naturalQuery.Query(dbContext.Orders,
    "pending orders sorted by date descending");
```

### Using via API

```
GET /api/natural-query?entity=Product&q=produits actifs dont le prix est superieur a 20
```

## Registration

```csharp
// AI Services
builder.Services.AddRvrAI(options =>
{
    options.Provider = "OpenAI";  // or "Azure", "Ollama"
    options.ApiKey = builder.Configuration["AI:ApiKey"];
    options.Model = "gpt-4o";
    options.EmbeddingModel = "text-embedding-3-small";
});

// Vector Store
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();

// RAG Pipeline
builder.Services.AddRvrRag();

// Natural Query
builder.Services.AddRvrNaturalQuery(options =>
{
    options.DefaultLanguage = "fr";
    options.SupportedLanguages = new[] { "fr", "en" };
});
```
