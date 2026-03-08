using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace KBA.Framework.AI.Tests;

/// <summary>
/// Unit tests for the <see cref="ChatMessage"/> class.
/// </summary>
public class ChatMessageTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesMessageWithDefaultValues()
    {
        // Act
        var message = new ChatMessage();

        // Assert
        message.Role.Should().Be(ChatRole.System);
        message.Content.Should().BeEmpty();
        message.ToolCalls.Should().BeEmpty();
        message.ToolCallId.Should().BeNull();
        message.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithRoleAndContent_SetsProperties()
    {
        // Arrange
        var role = ChatRole.User;
        var content = "Hello, world!";

        // Act
        var message = new ChatMessage(role, content);

        // Assert
        message.Role.Should().Be(role);
        message.Content.Should().Be(content);
    }

    #endregion

    #region Static Factory Methods Tests

    [Fact]
    public void CreateSystem_CreatesSystemMessage()
    {
        // Arrange
        var content = "You are a helpful assistant.";

        // Act
        var message = ChatMessage.CreateSystem(content);

        // Assert
        message.Role.Should().Be(ChatRole.System);
        message.Content.Should().Be(content);
    }

    [Fact]
    public void CreateUser_CreatesUserMessage()
    {
        // Arrange
        var content = "What is the weather?";

        // Act
        var message = ChatMessage.CreateUser(content);

        // Assert
        message.Role.Should().Be(ChatRole.User);
        message.Content.Should().Be(content);
    }

    [Fact]
    public void CreateAssistant_CreatesAssistantMessage()
    {
        // Arrange
        var content = "The weather is sunny.";

        // Act
        var message = ChatMessage.CreateAssistant(content);

        // Assert
        message.Role.Should().Be(ChatRole.Assistant);
        message.Content.Should().Be(content);
    }

    [Fact]
    public void CreateTool_CreatesToolMessage()
    {
        // Arrange
        var result = "{\"temperature\": 25}";
        var callId = "call_123";

        // Act
        var message = ChatMessage.CreateTool(result, callId);

        // Assert
        message.Role.Should().Be(ChatRole.Tool);
        message.Content.Should().Be(result);
        message.ToolCallId.Should().Be(callId);
    }

    #endregion

    #region ChatRole Enum Tests

    [Fact]
    public void ChatRole_HasExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(ChatRole)).Should().ContainInOrder(
            "System", "User", "Assistant", "Tool");
    }

    [Theory]
    [InlineData(ChatRole.System)]
    [InlineData(ChatRole.User)]
    [InlineData(ChatRole.Assistant)]
    [InlineData(ChatRole.Tool)]
    public void ChatRole_ValuesAreValid(ChatRole role)
    {
        // Assert
        Enum.IsDefined(typeof(ChatRole), role).Should().BeTrue();
    }

    #endregion

    #region ToolCall Tests

    [Fact]
    public void ToolCall_DefaultValues()
    {
        // Act
        var toolCall = new ToolCall();

        // Assert
        toolCall.Id.Should().BeEmpty();
        toolCall.Name.Should().BeEmpty();
        toolCall.Arguments.Should().BeEmpty();
        toolCall.Result.Should().BeNull();
    }

    [Fact]
    public void ToolCall_WithProperties_SetsValues()
    {
        // Arrange
        var toolCall = new ToolCall
        {
            Id = "call_123",
            Name = "get_weather",
            Arguments = "{\"location\": \"Paris\"}",
            Result = "{\"temperature\": 25}"
        };

        // Assert
        toolCall.Id.Should().Be("call_123");
        toolCall.Name.Should().Be("get_weather");
        toolCall.Arguments.Should().Be("{\"location\": \"Paris\"}");
        toolCall.Result.Should().Be("{\"temperature\": 25}");
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void MessageMetadata_CanStoreCustomData()
    {
        // Arrange
        var message = new ChatMessage();

        // Act
        message.Metadata["custom_key"] = "custom_value";
        message.Metadata["number"] = 42;

        // Assert
        message.Metadata.Should().ContainKey("custom_key").WhoseValue.Should().Be("custom_value");
        message.Metadata.Should().ContainKey("number").WhoseValue.Should().Be(42);
    }

    #endregion
}

/// <summary>
/// Unit tests for the <see cref="ChatRequest"/> class.
/// </summary>
public class ChatRequestTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesRequestWithDefaultValues()
    {
        // Act
        var request = new ChatRequest();

        // Assert
        request.CorrelationId.Should().NotBeEmpty();
        request.CorrelationId.Should().HaveLength(32); // GUID without dashes
        request.TenantId.Should().BeNull();
        request.Messages.Should().BeEmpty();
        request.Metadata.Should().BeEmpty();
        request.Options.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_GeneratesUniqueCorrelationIds()
    {
        // Act
        var request1 = new ChatRequest();
        var request2 = new ChatRequest();

        // Assert
        request1.CorrelationId.Should().NotBe(request2.CorrelationId);
    }

    #endregion

    #region Fluent Builder Methods Tests

    [Fact]
    public void WithSystemMessage_AddsSystemMessage()
    {
        // Arrange
        var request = new ChatRequest();
        var content = "You are a helpful assistant.";

        // Act
        var result = request.WithSystemMessage(content);

        // Assert
        result.Should().BeSameAs(request);
        request.Messages.Should().HaveCount(1);
        request.Messages[0].Role.Should().Be(ChatRole.System);
        request.Messages[0].Content.Should().Be(content);
    }

    [Fact]
    public void WithUserMessage_AddsUserMessage()
    {
        // Arrange
        var request = new ChatRequest();
        var content = "What is the weather?";

        // Act
        var result = request.WithUserMessage(content);

        // Assert
        result.Should().BeSameAs(request);
        request.Messages.Should().HaveCount(1);
        request.Messages[0].Role.Should().Be(ChatRole.User);
        request.Messages[0].Content.Should().Be(content);
    }

    [Fact]
    public void WithAssistantMessage_AddsAssistantMessage()
    {
        // Arrange
        var request = new ChatRequest();
        var content = "The weather is sunny.";

        // Act
        var result = request.WithAssistantMessage(content);

        // Assert
        result.Should().BeSameAs(request);
        request.Messages.Should().HaveCount(1);
        request.Messages[0].Role.Should().Be(ChatRole.Assistant);
        request.Messages[0].Content.Should().Be(content);
    }

    [Fact]
    public void WithMultipleMessages_BuildsConversation()
    {
        // Arrange
        var request = new ChatRequest();

        // Act
        request
            .WithSystemMessage("You are helpful.")
            .WithUserMessage("Hello!")
            .WithAssistantMessage("Hi there!");

        // Assert
        request.Messages.Should().HaveCount(3);
        request.Messages[0].Role.Should().Be(ChatRole.System);
        request.Messages[1].Role.Should().Be(ChatRole.User);
        request.Messages[2].Role.Should().Be(ChatRole.Assistant);
    }

    #endregion

    #region ChatOptions Tests

    [Fact]
    public void ChatOptions_DefaultValues()
    {
        // Arrange
        var options = new ChatOptions();

        // Assert
        options.MaxTokens.Should().Be(1024);
        options.Temperature.Should().Be(0.7);
        options.TopP.Should().Be(0.9);
        options.Seed.Should().BeNull();
        options.StopSequences.Should().BeEmpty();
        options.Timeout.Should().BeNull();
        options.RetryPolicy.Should().NotBeNull();
        options.IncludeUsage.Should().BeTrue();
        options.Tools.Should().BeEmpty();
        options.ToolChoice.Should().BeNull();
        options.FrequencyPenalty.Should().BeNull();
        options.PresencePenalty.Should().BeNull();
        options.ResponseFormatJson.Should().BeFalse();
    }

    [Fact]
    public void ChatOptions_CanBeConfigured()
    {
        // Arrange
        var options = new ChatOptions
        {
            MaxTokens = 2048,
            Temperature = 0.5,
            TopP = 0.8,
            Seed = 42,
            StopSequences = new List<string> { "\n", "END" },
            Timeout = TimeSpan.FromSeconds(30),
            IncludeUsage = false,
            ResponseFormatJson = true,
            FrequencyPenalty = 0.5,
            PresencePenalty = 0.3
        };

        // Assert
        options.MaxTokens.Should().Be(2048);
        options.Temperature.Should().Be(0.5);
        options.TopP.Should().Be(0.8);
        options.Seed.Should().Be(42);
        options.StopSequences.Should().HaveCount(2);
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.IncludeUsage.Should().BeFalse();
        options.ResponseFormatJson.Should().BeTrue();
        options.FrequencyPenalty.Should().Be(0.5);
        options.PresencePenalty.Should().Be(0.3);
    }

    #endregion

    #region RetryPolicy Tests

    [Fact]
    public void RetryPolicy_DefaultValues()
    {
        // Arrange
        var policy = new RetryPolicy();

        // Assert
        policy.MaxRetries.Should().Be(3);
        policy.InitialDelayMs.Should().Be(1000);
        policy.MaxDelayMs.Should().Be(30000);
        policy.BackoffMultiplier.Should().Be(2.0);
        policy.UseJitter.Should().BeTrue();
    }

    [Fact]
    public void RetryPolicy_CanBeConfigured()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxRetries = 5,
            InitialDelayMs = 500,
            MaxDelayMs = 60000,
            BackoffMultiplier = 1.5,
            UseJitter = false
        };

        // Assert
        policy.MaxRetries.Should().Be(5);
        policy.InitialDelayMs.Should().Be(500);
        policy.MaxDelayMs.Should().Be(60000);
        policy.BackoffMultiplier.Should().Be(1.5);
        policy.UseJitter.Should().BeFalse();
    }

    #endregion

    #region ChatTool Tests

    [Fact]
    public void ChatTool_DefaultValues()
    {
        // Arrange
        var tool = new ChatTool();

        // Assert
        tool.Name.Should().BeEmpty();
        tool.Description.Should().BeEmpty();
        tool.ParametersJsonSchema.Should().BeEmpty();
    }

    [Fact]
    public void ChatTool_CanBeConfigured()
    {
        // Arrange
        var tool = new ChatTool
        {
            Name = "get_weather",
            Description = "Gets the weather for a location",
            ParametersJsonSchema = "{\"type\": \"object\", \"properties\": {\"location\": {\"type\": \"string\"}}}"
        };

        // Assert
        tool.Name.Should().Be("get_weather");
        tool.Description.Should().Be("Gets the weather for a location");
        tool.ParametersJsonSchema.Should().Contain("location");
    }

    #endregion

    #region FinishReason Enum Tests

    [Fact]
    public void FinishReason_HasExpectedValues()
    {
        // Assert
        Enum.GetNames(typeof(FinishReason)).Should().Contain(
            "Stop", "Length", "ContentFilter", "ToolCall", "Cancelled", "Unknown");
    }

    #endregion
}
