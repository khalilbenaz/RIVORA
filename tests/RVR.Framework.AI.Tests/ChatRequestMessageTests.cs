namespace RVR.Framework.AI.Tests;

using FluentAssertions;
using RVR.Framework.AI.Abstractions;

public class ChatRequestMessageTests
{
    [Fact]
    public void ChatMessage_DefaultRole_ShouldBeUser()
    {
        var message = new ChatMessage();
        message.Role.Should().Be("user");
    }

    [Fact]
    public void ChatMessage_ShouldStoreContent()
    {
        var message = new ChatMessage { Role = "assistant", Content = "Hello!" };
        message.Role.Should().Be("assistant");
        message.Content.Should().Be("Hello!");
    }

    [Fact]
    public void ChatRequest_DefaultValues_ShouldBeCorrect()
    {
        var request = new ChatRequest();
        request.Model.Should().BeEmpty();
        request.Messages.Should().BeEmpty();
        request.Temperature.Should().Be(0.7f);
        request.MaxTokens.Should().Be(1024);
        request.SystemPrompt.Should().BeNull();
    }

    [Fact]
    public void ChatRequest_ShouldAcceptMessages()
    {
        var request = new ChatRequest
        {
            Model = "gpt-4",
            SystemPrompt = "You are a helpful assistant.",
            Messages =
            [
                new() { Role = "user", Content = "Hello" },
                new() { Role = "assistant", Content = "Hi there!" },
                new() { Role = "user", Content = "How are you?" }
            ]
        };

        request.Messages.Should().HaveCount(3);
        request.Messages[0].Role.Should().Be("user");
        request.Messages[1].Role.Should().Be("assistant");
    }

    [Fact]
    public void ChatResponse_ShouldStoreTokenCounts()
    {
        var response = new ChatResponse
        {
            Content = "Hello!",
            Model = "gpt-4",
            PromptTokens = 10,
            CompletionTokens = 5,
            TotalTokens = 15
        };

        response.TotalTokens.Should().Be(15);
    }

    [Theory]
    [InlineData("system")]
    [InlineData("user")]
    [InlineData("assistant")]
    public void ChatMessage_ShouldAcceptStandardRoles(string role)
    {
        var message = new ChatMessage { Role = role, Content = "test" };
        message.Role.Should().Be(role);
    }
}
