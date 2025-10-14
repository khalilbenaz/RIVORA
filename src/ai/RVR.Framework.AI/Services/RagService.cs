using RVR.Framework.AI.Abstractions;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.AI.Services;

/// <summary>
/// Provides Retrieval-Augmented Generation capabilities.
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Ingests a document by chunking it, generating embeddings, and storing in the vector store.
    /// </summary>
    Task IngestDocumentAsync(string documentId, string content, Dictionary<string, string>? metadata = null, CancellationToken ct = default);

    /// <summary>
    /// Queries the knowledge base and generates an answer using retrieved context.
    /// </summary>
    Task<RagResponse> QueryAsync(string question, RagOptions? options = null, CancellationToken ct = default);
}

/// <summary>
/// Represents the response from a RAG query.
/// </summary>
public class RagResponse
{
    /// <summary>
    /// The generated answer text.
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The source documents that were used to generate the answer.
    /// </summary>
    public List<RagSource> Sources { get; set; } = [];

    /// <summary>
    /// The raw chat response from the language model, if available.
    /// </summary>
    public ChatResponse? RawResponse { get; set; }
}

/// <summary>
/// Represents a source document used in a RAG response.
/// </summary>
public class RagSource
{
    /// <summary>
    /// The identifier of the source document.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// The content of the relevant chunk.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The similarity score of this source to the query.
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Metadata associated with this source.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];
}

/// <summary>
/// Options for controlling RAG query behavior.
/// </summary>
public class RagOptions
{
    /// <summary>
    /// The number of top results to retrieve from the vector store.
    /// </summary>
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Optional system prompt override for the language model.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Sampling temperature for the language model.
    /// </summary>
    public float Temperature { get; set; } = 0.3f;
}

/// <summary>
/// Default implementation of <see cref="IRagService"/> that orchestrates document ingestion and query answering.
/// </summary>
public class RagService : IRagService
{
    private readonly IChatClient _chatClient;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly IDocumentChunker _chunker;
    private readonly ILogger<RagService> _logger;

    private const string DefaultSystemPrompt =
        """
        You are a helpful assistant. Answer the user's question based on the provided context.
        If the context does not contain enough information to answer the question, say so clearly.
        Always cite which parts of the context you used in your answer.

        Context:
        {context}
        """;

    /// <summary>
    /// Initializes a new instance of <see cref="RagService"/>.
    /// </summary>
    public RagService(
        IChatClient chatClient,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IDocumentChunker chunker,
        ILogger<RagService> logger)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task IngestDocumentAsync(
        string documentId,
        string content,
        Dictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        _logger.LogInformation("Ingesting document {DocumentId} ({Length} chars)", documentId, content.Length);

        var chunks = _chunker.ChunkText(content);
        _logger.LogDebug("Document {DocumentId} split into {ChunkCount} chunks", documentId, chunks.Count);

        var texts = chunks.Select(c => c.Content).ToList();
        var embeddings = await _embeddingService.GetEmbeddingsAsync(texts, ct);

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunkId = $"{documentId}::chunk-{i}";
            var chunkMetadata = new Dictionary<string, string>(metadata ?? [])
            {
                ["documentId"] = documentId,
                ["chunkIndex"] = i.ToString(),
                ["content"] = chunks[i].Content,
                ["startOffset"] = chunks[i].StartOffset.ToString(),
                ["endOffset"] = chunks[i].EndOffset.ToString()
            };

            await _vectorStore.UpsertAsync(chunkId, embeddings[i], chunkMetadata, ct);
        }

        _logger.LogInformation("Document {DocumentId} ingested successfully ({ChunkCount} chunks)", documentId, chunks.Count);
    }

    /// <inheritdoc />
    public async Task<RagResponse> QueryAsync(
        string question,
        RagOptions? options = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        options ??= new RagOptions();

        _logger.LogInformation("Processing RAG query: {Question}", question);

        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GetEmbeddingAsync(question, ct);

        // Search the vector store for relevant chunks
        var searchResults = await _vectorStore.SearchAsync(queryEmbedding, options.TopK, ct);

        _logger.LogDebug("Found {ResultCount} relevant chunks", searchResults.Count);

        // Build sources from search results
        var sources = searchResults.Select(r => new RagSource
        {
            DocumentId = r.Metadata.GetValueOrDefault("documentId", r.Id),
            Content = r.Metadata.GetValueOrDefault("content", string.Empty),
            Score = r.Score,
            Metadata = r.Metadata
        }).ToList();

        // Build context from retrieved chunks
        var contextParts = searchResults
            .Select((r, idx) =>
            {
                var docId = r.Metadata.GetValueOrDefault("documentId", "unknown");
                var chunkContent = r.Metadata.GetValueOrDefault("content", string.Empty);
                return $"[Source {idx + 1} - {docId} (score: {r.Score:F3})]:\n{chunkContent}";
            });

        var context = string.Join("\n\n", contextParts);

        // Build the system prompt with context
        var systemPrompt = (options.SystemPrompt ?? DefaultSystemPrompt)
            .Replace("{context}", context);

        // Call the chat client
        var chatRequest = new ChatRequest
        {
            Messages =
            [
                new ChatMessage { Role = "user", Content = question }
            ],
            Temperature = options.Temperature,
            SystemPrompt = systemPrompt
        };

        var chatResponse = await _chatClient.ChatAsync(chatRequest, ct);

        _logger.LogInformation(
            "RAG query completed. Tokens used: {TotalTokens}",
            chatResponse.TotalTokens);

        return new RagResponse
        {
            Answer = chatResponse.Content,
            Sources = sources,
            RawResponse = chatResponse
        };
    }
}
