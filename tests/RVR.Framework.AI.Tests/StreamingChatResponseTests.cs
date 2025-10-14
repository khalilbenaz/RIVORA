namespace RVR.Framework.AI.Tests;

using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Services;

public class RagServiceTests
{
    [Fact]
    public async Task IngestDocument_ShouldChunkAndStore()
    {
        var mockChat = Substitute.For<IChatClient>();
        var mockEmbed = Substitute.For<IEmbeddingService>();
        var store = new InMemoryVectorStore();
        var chunker = new SlidingWindowChunker();
        var logger = Substitute.For<ILogger<RagService>>();

        mockEmbed.GetEmbeddingsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var texts = ci.Arg<IEnumerable<string>>().ToList();
                IReadOnlyList<float[]> r = texts.Select(_ => new float[] { 1f, 0f, 0f }).ToList();
                return Task.FromResult(r);
            });

        var rag = new RagService(mockChat, mockEmbed, store, chunker, logger);
        await rag.IngestDocumentAsync("doc1", "This is a test document with enough content.");

        var results = await store.SearchAsync([1f, 0f, 0f], topK: 10);
        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Query_ShouldReturnAnswer()
    {
        var mockChat = Substitute.For<IChatClient>();
        var mockEmbed = Substitute.For<IEmbeddingService>();
        var store = new InMemoryVectorStore();
        var chunker = new SlidingWindowChunker();
        var logger = Substitute.For<ILogger<RagService>>();

        mockEmbed.GetEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new float[] { 1f, 0f, 0f });
        mockEmbed.GetEmbeddingsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var texts = ci.Arg<IEnumerable<string>>().ToList();
                IReadOnlyList<float[]> r = texts.Select(_ => new float[] { 1f, 0f, 0f }).ToList();
                return Task.FromResult(r);
            });
        mockChat.ChatAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse { Content = "42", Model = "test" });

        var rag = new RagService(mockChat, mockEmbed, store, chunker, logger);
        await rag.IngestDocumentAsync("doc1", "The meaning of life is 42.");

        var result = await rag.QueryAsync("What is the meaning of life?");
        result.Answer.Should().Be("42");
        result.Sources.Should().NotBeEmpty();
    }

    [Fact]
    public void VectorSearchResult_Properties()
    {
        var r = new VectorSearchResult { Id = "x", Score = 0.95f, Metadata = new() { ["k"] = "v" } };
        r.Score.Should().BeGreaterThan(0.9f);
        r.Metadata["k"].Should().Be("v");
    }
}
