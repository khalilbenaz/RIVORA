using FluentAssertions;
using Xunit;

namespace KBA.Framework.AI.Tests;

/// <summary>
/// Unit tests for the <see cref="StreamingChatResponse"/> class.
/// </summary>
public class StreamingChatResponseTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange
        var response = new StreamingChatResponse();

        // Assert
        response.CorrelationId.Should().BeEmpty();
        response.DeltaContent.Should().BeEmpty();
        response.Role.Should().BeNull();
        response.ToolCallDeltas.Should().BeEmpty();
        response.FinishReason.Should().BeNull();
        response.Usage.Should().BeNull();
        response.Model.Should().BeEmpty();
        response.ChunkIndex.Should().Be(0);
        response.IsFinalChunk.Should().BeFalse();
        response.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var response = new StreamingChatResponse
        {
            CorrelationId = "test-correlation-id",
            DeltaContent = "Hello",
            Role = ChatRole.Assistant,
            FinishReason = FinishReason.Stop,
            Usage = new TokenUsage { InputTokens = 10, OutputTokens = 5 },
            Model = "gpt-4o",
            ChunkIndex = 5,
            IsFinalChunk = true
        };

        response.ToolCallDeltas.Add(new ToolCallDelta
        {
            Index = 0,
            Id = "call_123",
            Name = "get_weather",
            ArgumentsDelta = "{\"location"
        });

        response.Metadata["custom"] = "value";

        // Assert
        response.CorrelationId.Should().Be("test-correlation-id");
        response.DeltaContent.Should().Be("Hello");
        response.Role.Should().Be(ChatRole.Assistant);
        response.ToolCallDeltas.Should().HaveCount(1);
        response.FinishReason.Should().Be(FinishReason.Stop);
        response.Usage.Should().NotBeNull();
        response.Model.Should().Be("gpt-4o");
        response.ChunkIndex.Should().Be(5);
        response.IsFinalChunk.Should().BeTrue();
        response.Metadata.Should().ContainKey("custom").WhoseValue.Should().Be("value");
    }

    [Fact]
    public void ToolCallDeltas_IsInitialized()
    {
        // Arrange
        var response = new StreamingChatResponse();

        // Assert
        response.ToolCallDeltas.Should().NotBeNull();
        response.ToolCallDeltas.Should().BeEmpty();
    }

    [Fact]
    public void Metadata_IsInitialized()
    {
        // Arrange
        var response = new StreamingChatResponse();

        // Assert
        response.Metadata.Should().NotBeNull();
        response.Metadata.Should().BeEmpty();
    }
}

/// <summary>
/// Unit tests for the <see cref="ToolCallDelta"/> class.
/// </summary>
public class ToolCallDeltaTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange
        var delta = new ToolCallDelta();

        // Assert
        delta.Index.Should().Be(0);
        delta.Id.Should().BeNull();
        delta.Name.Should().BeNull();
        delta.ArgumentsDelta.Should().BeNull();
        delta.AccumulatedArguments.Should().BeNull();
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var delta = new ToolCallDelta
        {
            Index = 1,
            Id = "call_123",
            Name = "get_weather",
            ArgumentsDelta = "{\"location\": \"Paris\"}",
            AccumulatedArguments = "{\"location\": \"Paris\"}"
        };

        // Assert
        delta.Index.Should().Be(1);
        delta.Id.Should().Be("call_123");
        delta.Name.Should().Be("get_weather");
        delta.ArgumentsDelta.Should().Be("{\"location\": \"Paris\"}");
        delta.AccumulatedArguments.Should().Be("{\"location\": \"Paris\"}");
    }
}

/// <summary>
/// Unit tests for the <see cref="RateLimitException"/> class.
/// </summary>
public class RateLimitExceptionTests
{
    [Fact]
    public void Constructor_DefaultMessage()
    {
        // Act
        var ex = new RateLimitException();

        // Assert
        ex.Message.Should().Be("Rate limit exceeded.");
        ex.RetryAfter.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithCustomMessage()
    {
        // Arrange
        var message = "Custom rate limit message";

        // Act
        var ex = new RateLimitException(message);

        // Assert
        ex.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithInnerException()
    {
        // Arrange
        var message = "Custom message";
        var inner = new System.Exception("Inner exception");

        // Act
        var ex = new RateLimitException(message, inner);

        // Assert
        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void RetryAfter_CanBeSet()
    {
        // Arrange
        var ex = new RateLimitException
        {
            RetryAfter = TimeSpan.FromSeconds(30)
        };

        // Assert
        ex.RetryAfter.Should().Be(TimeSpan.FromSeconds(30));
    }
}

/// <summary>
/// Unit tests for the <see cref="TransientException"/> class.
/// </summary>
public class TransientExceptionTests
{
    [Fact]
    public void Constructor_DefaultMessage()
    {
        // Act
        var ex = new TransientException();

        // Assert
        ex.Message.Should().Be("A transient error occurred.");
    }

    [Fact]
    public void Constructor_WithCustomMessage()
    {
        // Arrange
        var message = "Custom transient error message";

        // Act
        var ex = new TransientException(message);

        // Assert
        ex.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithInnerException()
    {
        // Arrange
        var message = "Custom message";
        var inner = new System.Exception("Inner exception");

        // Act
        var ex = new TransientException(message, inner);

        // Assert
        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(inner);
    }
}

/// <summary>
/// Unit tests for the <see cref="OpenAiOptions"/> class.
/// </summary>
public class OpenAiOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange
        var options = new OpenAiOptions();

        // Assert
        options.ApiKey.Should().BeEmpty();
        options.OrganizationId.Should().BeNull();
        options.ProjectId.Should().BeNull();
        options.Endpoint.Should().Be("https://api.openai.com/v1");
        options.ModelId.Should().Be("gpt-4o");
        options.TimeoutSeconds.Should().Be(100);
        options.DefaultResponseFormatJson.Should().BeFalse();
        options.DefaultMaxTokens.Should().Be(1024);
        options.DefaultTemperature.Should().Be(0.7);
        options.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var options = new OpenAiOptions
        {
            ApiKey = "sk-test123",
            OrganizationId = "org-123",
            ProjectId = "proj-456",
            Endpoint = "https://custom-endpoint.com/v1",
            ModelId = "gpt-4-turbo",
            TimeoutSeconds = 60,
            DefaultResponseFormatJson = true,
            DefaultMaxTokens = 2048,
            DefaultTemperature = 0.5,
            IsEnabled = false
        };

        // Assert
        options.ApiKey.Should().Be("sk-test123");
        options.OrganizationId.Should().Be("org-123");
        options.ProjectId.Should().Be("proj-456");
        options.Endpoint.Should().Be("https://custom-endpoint.com/v1");
        options.ModelId.Should().Be("gpt-4-turbo");
        options.TimeoutSeconds.Should().Be(60);
        options.DefaultResponseFormatJson.Should().BeTrue();
        options.DefaultMaxTokens.Should().Be(2048);
        options.DefaultTemperature.Should().Be(0.5);
        options.IsEnabled.Should().BeFalse();
    }
}
