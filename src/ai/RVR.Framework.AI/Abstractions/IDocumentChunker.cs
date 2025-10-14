namespace RVR.Framework.AI.Abstractions;

/// <summary>
/// Abstraction for splitting documents into smaller chunks for embedding.
/// </summary>
public interface IDocumentChunker
{
    /// <summary>
    /// Splits the input text into chunks according to the specified options.
    /// </summary>
    IReadOnlyList<DocumentChunk> ChunkText(string text, ChunkingOptions? options = null);
}

/// <summary>
/// Represents a single chunk of a document.
/// </summary>
public class DocumentChunk
{
    /// <summary>
    /// The text content of the chunk.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The zero-based index of this chunk within the document.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The character offset where this chunk starts in the original text.
    /// </summary>
    public int StartOffset { get; set; }

    /// <summary>
    /// The character offset where this chunk ends in the original text.
    /// </summary>
    public int EndOffset { get; set; }
}

/// <summary>
/// Options for controlling how text is chunked.
/// </summary>
public class ChunkingOptions
{
    /// <summary>
    /// The target size of each chunk in characters.
    /// </summary>
    public int ChunkSize { get; set; } = 512;

    /// <summary>
    /// The number of characters to overlap between consecutive chunks.
    /// </summary>
    public int ChunkOverlap { get; set; } = 50;
}
