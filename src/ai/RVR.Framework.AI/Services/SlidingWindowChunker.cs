using System.Text.RegularExpressions;
using RVR.Framework.AI.Abstractions;

namespace RVR.Framework.AI.Services;

/// <summary>
/// A document chunker that uses a sliding window approach over sentence boundaries.
/// Sentences are detected using common punctuation patterns, and chunks are formed
/// by accumulating sentences up to the target chunk size with configurable overlap.
/// </summary>
public partial class SlidingWindowChunker : IDocumentChunker
{
    /// <inheritdoc />
    public IReadOnlyList<DocumentChunk> ChunkText(string text, ChunkingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text))
            return [];

        options ??= new ChunkingOptions();

        if (options.ChunkSize <= 0)
            throw new ArgumentException("ChunkSize must be greater than zero.", nameof(options));

        if (options.ChunkOverlap < 0)
            throw new ArgumentException("ChunkOverlap must not be negative.", nameof(options));

        if (options.ChunkOverlap >= options.ChunkSize)
            throw new ArgumentException("ChunkOverlap must be less than ChunkSize.", nameof(options));

        var sentences = SplitIntoSentences(text);

        if (sentences.Count == 0)
            return [];

        var chunks = new List<DocumentChunk>();
        var currentStart = 0;

        while (currentStart < sentences.Count)
        {
            var currentLength = 0;
            var currentEnd = currentStart;

            // Accumulate sentences until we reach the chunk size
            while (currentEnd < sentences.Count)
            {
                var sentenceLength = sentences[currentEnd].Text.Length;
                if (currentLength > 0 && currentLength + sentenceLength > options.ChunkSize)
                    break;

                currentLength += sentenceLength;
                currentEnd++;
            }

            // Ensure we include at least one sentence
            if (currentEnd == currentStart)
                currentEnd = currentStart + 1;

            var chunkContent = string.Concat(
                sentences.Skip(currentStart).Take(currentEnd - currentStart).Select(s => s.Text));

            var startOffset = sentences[currentStart].Offset;
            var lastSentence = sentences[currentEnd - 1];
            var endOffset = lastSentence.Offset + lastSentence.Text.Length;

            chunks.Add(new DocumentChunk
            {
                Content = chunkContent,
                Index = chunks.Count,
                StartOffset = startOffset,
                EndOffset = endOffset
            });

            if (currentEnd >= sentences.Count)
                break;

            // Move forward, but back up by the overlap amount
            var overlapLength = 0;
            var overlapStart = currentEnd;
            while (overlapStart > currentStart && overlapLength < options.ChunkOverlap)
            {
                overlapStart--;
                overlapLength += sentences[overlapStart].Text.Length;
            }

            currentStart = overlapStart < currentEnd ? overlapStart : currentEnd;

            // Prevent infinite loop: always advance at least one sentence
            if (currentStart <= chunks[^1].Index && currentStart < currentEnd)
                currentStart = Math.Max(currentStart, currentEnd - (int)Math.Ceiling((double)options.ChunkOverlap / options.ChunkSize * (currentEnd - currentStart)));

            // Absolute safety: always advance by at least one sentence beyond where previous chunk started
            if (chunks.Count >= 2)
            {
                // Just ensure forward progress
                var prevChunkStartSentence = FindSentenceIndexByOffset(sentences, chunks[^2].StartOffset);
                if (currentStart <= prevChunkStartSentence)
                    currentStart = prevChunkStartSentence + 1;
            }
        }

        return chunks;
    }

    private static int FindSentenceIndexByOffset(List<SentenceSpan> sentences, int offset)
    {
        for (var i = 0; i < sentences.Count; i++)
        {
            if (sentences[i].Offset >= offset)
                return i;
        }
        return sentences.Count - 1;
    }

    private static List<SentenceSpan> SplitIntoSentences(string text)
    {
        var sentences = new List<SentenceSpan>();
        var pattern = SentenceBoundaryRegex();

        var lastEnd = 0;
        foreach (Match match in pattern.Matches(text))
        {
            var end = match.Index + match.Length;
            var sentenceText = text[lastEnd..end];
            if (!string.IsNullOrWhiteSpace(sentenceText))
            {
                sentences.Add(new SentenceSpan(sentenceText, lastEnd));
            }
            lastEnd = end;
        }

        // Add any remaining text as a final sentence
        if (lastEnd < text.Length)
        {
            var remaining = text[lastEnd..];
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                sentences.Add(new SentenceSpan(remaining, lastEnd));
            }
        }

        return sentences;
    }

    [GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.Compiled)]
    private static partial Regex SentenceBoundaryRegex();

    private readonly record struct SentenceSpan(string Text, int Offset);
}
