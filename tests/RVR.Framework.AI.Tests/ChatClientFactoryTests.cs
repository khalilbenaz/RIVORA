namespace RVR.Framework.AI.Tests;

using FluentAssertions;
using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Services;

public class InMemoryVectorStoreTests
{
    private readonly InMemoryVectorStore _store = new();

    [Fact]
    public async Task Upsert_ShouldStoreAndRetrieve()
    {
        await _store.UpsertAsync("doc1", [1.0f, 0.0f, 0.0f], new() { ["source"] = "test" });

        var results = await _store.SearchAsync([1.0f, 0.0f, 0.0f], topK: 1);
        results.Should().HaveCount(1);
        results[0].Id.Should().Be("doc1");
        results[0].Score.Should().BeGreaterThan(0.99f);
    }

    [Fact]
    public async Task Search_ShouldReturnMostSimilarFirst()
    {
        await _store.UpsertAsync("a", [1.0f, 0.0f, 0.0f], new() { ["label"] = "x" });
        await _store.UpsertAsync("b", [0.0f, 1.0f, 0.0f], new() { ["label"] = "y" });
        await _store.UpsertAsync("c", [0.0f, 0.0f, 1.0f], new() { ["label"] = "z" });

        var results = await _store.SearchAsync([0.9f, 0.1f, 0.0f], topK: 2);
        results[0].Id.Should().Be("a");
    }

    [Fact]
    public async Task Delete_ShouldRemoveVector()
    {
        await _store.UpsertAsync("doc1", [1.0f, 0.0f], new());
        await _store.DeleteAsync("doc1");

        var results = await _store.SearchAsync([1.0f, 0.0f]);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Upsert_SameId_ShouldOverwrite()
    {
        await _store.UpsertAsync("doc1", [1.0f, 0.0f], new() { ["v"] = "1" });
        await _store.UpsertAsync("doc1", [0.0f, 1.0f], new() { ["v"] = "2" });

        var results = await _store.SearchAsync([0.0f, 1.0f], topK: 1);
        results[0].Metadata["v"].Should().Be("2");
    }

    [Fact]
    public async Task Search_EmptyStore_ShouldReturnEmpty()
    {
        var results = await _store.SearchAsync([1.0f, 0.0f]);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_TopK_ShouldLimitResults()
    {
        for (int i = 0; i < 10; i++)
        {
            var embedding = new float[3];
            embedding[i % 3] = 1.0f;
            await _store.UpsertAsync($"doc{i}", embedding, new());
        }

        var results = await _store.SearchAsync([1.0f, 0.0f, 0.0f], topK: 3);
        results.Should().HaveCount(3);
    }
}
