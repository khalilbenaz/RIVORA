namespace RVR.Framework.AI.Tests;

using FluentAssertions;
using NSubstitute;
using RVR.Framework.AI.Abstractions;

public class ChatClientTests
{
    [Fact]
    public async Task IChatClient_ChatAsync_ShouldReturnResponse()
    {
        var mockClient = Substitute.For<IChatClient>();
        mockClient.ChatAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse { Content = "Hello!", Model = "test", TotalTokens = 13 });

        var response = await mockClient.ChatAsync(new ChatRequest
        {
            Messages = [new() { Role = "user", Content = "Hello" }]
        });

        response.Content.Should().Be("Hello!");
        response.TotalTokens.Should().Be(13);
    }

    [Fact]
    public async Task IChatClient_StreamChatAsync_ShouldReturnChunks()
    {
        var mockClient = Substitute.For<IChatClient>();

        async IAsyncEnumerable<string> MockStream()
        {
            yield return "Hello";
            yield return " world";
            await Task.CompletedTask;
        }

        mockClient.StreamChatAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(MockStream() as IAsyncEnumerable<string>));

        var stream = await mockClient.StreamChatAsync(new ChatRequest());
        var chunks = new List<string>();
        await foreach (var chunk in stream)
            chunks.Add(chunk);

        string.Join("", chunks).Should().Be("Hello world");
    }
}
