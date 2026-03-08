using System.Text;
using Microsoft.Extensions.Logging;
using KBA.AI.RAG.App.Domain.Entities;

namespace KBA.AI.RAG.App.RAG;

public interface IRagPipeline
{
    Task<List<DocumentChunk>> ProcessDocumentAsync(Document document, CancellationToken ct = default);
    Task<List<DocumentChunk>> ExtractTextFromPdfAsync(Stream stream, string fileName, CancellationToken ct = default);
    List<DocumentChunk> ChunkText(string text, int chunkSize = 500, int overlap = 50);
}

public class DocumentChunk
{
    public string Content { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string EmbeddingId { get; set; } = string.Empty;
}

public class RagPipeline : IRagPipeline
{
    private readonly ILogger<RagPipeline> _logger;

    public RagPipeline(ILogger<RagPipeline> logger)
    {
        _logger = logger;
    }

    public async Task<List<DocumentChunk>> ProcessDocumentAsync(Document document, CancellationToken ct = default)
    {
        _logger.LogInformation($"Processing document: {document.FileName}");
        
        List<DocumentChunk> chunks;
        
        if (document.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            // PDF processing would go here
            chunks = new List<DocumentChunk>
            {
                new DocumentChunk
                {
                    Content = document.Content,
                    DocumentId = document.Id.ToString(),
                    DocumentTitle = document.Title,
                    ChunkIndex = 0,
                    Metadata = new Dictionary<string, string>
                    {
                        { "document_id", document.Id.ToString() },
                        { "title", document.Title },
                        { "file_name", document.FileName }
                    }
                }
            };
        }
        else
        {
            chunks = ChunkText(document.Content);
            foreach (var chunk in chunks)
            {
                chunk.DocumentId = document.Id.ToString();
                chunk.DocumentTitle = document.Title;
                chunk.Metadata = new Dictionary<string, string>
                {
                    { "document_id", document.Id.ToString() },
                    { "title", document.Title }
                };
            }
        }
        
        return chunks;
    }

    public async Task<List<DocumentChunk>> ExtractTextFromPdfAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        // TODO: Implement actual PDF extraction using PdfPig
        var content = await ReadStreamAsync(stream, ct);
        
        return new List<DocumentChunk>
        {
            new DocumentChunk
            {
                Content = content,
                DocumentTitle = fileName,
                ChunkIndex = 0,
                Metadata = new Dictionary<string, string> { { "file_name", fileName } }
            }
        };
    }

    public List<DocumentChunk> ChunkText(string text, int chunkSize = 500, int overlap = 50)
    {
        var chunks = new List<DocumentChunk>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunkIndex = 0;
        
        for (int i = 0; i < words.Length; i += chunkSize - overlap)
        {
            var chunk = string.Join(" ", words.Skip(i).Take(chunkSize));
            chunks.Add(new DocumentChunk
            {
                Content = chunk,
                ChunkIndex = chunkIndex++,
                Metadata = new Dictionary<string, string>()
            });
        }
        
        return chunks;
    }

    private static async Task<string> ReadStreamAsync(Stream stream, CancellationToken ct)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync(ct);
    }
}
