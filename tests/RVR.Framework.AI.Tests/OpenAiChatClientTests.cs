namespace RVR.Framework.AI.Tests;

using FluentAssertions;
using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Services;

public class SlidingWindowChunkerTests
{
    private readonly SlidingWindowChunker _chunker = new();

    [Fact]
    public void ChunkText_ShortText_ShouldReturnSingleChunk()
    {
        var chunks = _chunker.ChunkText("Hello world.");
        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Be("Hello world.");
    }

    [Fact]
    public void ChunkText_LongText_ShouldSplit()
    {
        var text = string.Join(". ", Enumerable.Range(1, 100).Select(i => $"Sentence number {i}"));
        var chunks = _chunker.ChunkText(text, new ChunkingOptions { ChunkSize = 200, ChunkOverlap = 20 });

        chunks.Count.Should().BeGreaterThan(1);
        foreach (var chunk in chunks)
            chunk.Content.Should().NotBeEmpty();
    }

    [Fact]
    public void ChunkText_EmptyText_ShouldReturnEmpty()
    {
        _chunker.ChunkText("").Should().BeEmpty();
    }

    [Fact]
    public void ChunkText_ChunksAreOrdered()
    {
        var text = string.Join(" ", Enumerable.Range(1, 50).Select(i => $"Sentence {i} here."));
        var chunks = _chunker.ChunkText(text, new ChunkingOptions { ChunkSize = 100, ChunkOverlap = 20 });

        for (int i = 1; i < chunks.Count; i++)
            chunks[i].Index.Should().Be(chunks[i - 1].Index + 1);
    }

    [Fact]
    public void ChunkText_DefaultOptions_Uses512()
    {
        // Use text with sentence boundaries so the chunker can split it.
        // Default ChunkSize is 512, so ~1500 chars of sentences should yield multiple chunks.
        var text = string.Join(". ", Enumerable.Range(1, 100).Select(i => $"Sentence number {i}")) + ".";
        var chunks = _chunker.ChunkText(text);
        chunks.Count.Should().BeGreaterThan(1);
    }
}
