using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KBA.Framework.AI.Tests;

/// <summary>
/// Unit tests for the <see cref="ChatClientBase"/> class.
/// </summary>
public class ChatClientBaseTests
{
    #region Test Implementation

    private class TestChatClient : ChatClientBase
    {
        public override string ProviderName => "TestProvider";

        public TestChatClient(ILogger<ChatClientBase> logger, ChatClientMetadata metadata)
            : base(logger, metadata)
        {
        }

        protected override Task<ChatResponse> SendRequestAsync(ChatRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ChatResponse
            {
                Message = new ChatMessage(ChatRole.Assistant, "Test response"),
                Usage = new TokenUsage { InputTokens = 10, OutputTokens = 5 },
                Model = "test-model"
            });
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var metadata = new ChatClientMetadata();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestChatClient(null!, metadata));
    }

    [Fact]
    public void Constructor_WithNullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestChatClient(logger, null!));
    }

    [Fact]
    public void Constructor_SetsProviderNameAndMetadata()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata
        {
            ModelId = "test-model",
            ContextWindow = 8192
        };

        // Act
        var client = new TestChatClient(logger, metadata);

        // Assert
        client.ProviderName.Should().Be("TestProvider");
        client.Metadata.Should().Be(metadata);
    }

    #endregion

    #region CompleteAsync Tests

    [Fact]
    public async Task CompleteAsync_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest
        {
            Messages = { ChatMessage.CreateUser("Hello") }
        };

        // Act
        var response = await client.CompleteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Message.Role.Should().Be(ChatRole.Assistant);
        response.Message.Content.Should().Be("Test response");
        response.Model.Should().Be("test-model");
    }

    [Fact]
    public async Task CompleteAsync_SetsCorrelationIdOnResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest
        {
            Messages = { ChatMessage.CreateUser("Hello") }
        };

        // Act
        var response = await client.CompleteAsync(request);

        // Assert
        response.CorrelationId.Should().Be(request.CorrelationId);
    }

    [Fact]
    public async Task CompleteAsync_SetsLatencyOnResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest
        {
            Messages = { ChatMessage.CreateUser("Hello") }
        };

        // Act
        var response = await client.CompleteAsync(request);

        // Assert
        response.Latency.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task CompleteAsync_SetsAttemptNumberOnResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest
        {
            Messages = { ChatMessage.CreateUser("Hello") }
        };

        // Act
        var response = await client.CompleteAsync(request);

        // Assert
        response.AttemptNumber.Should().Be(1);
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyMessages_ThrowsArgumentException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.CompleteAsync(request));
    }

    [Fact]
    public async Task CompleteAsync_WithInvalidMaxTokens_ThrowsArgumentException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest
        {
            Messages = { ChatMessage.CreateUser("Hello") },
            Options = { MaxTokens = 0 }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.CompleteAsync(request));
    }

    [Fact]
    public async Task CompleteAsync_WithInvalidTemperature_ThrowsArgumentException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var request = new ChatRequest
        {
            Messages = { ChatMessage.CreateUser("Hello") },
            Options = { Temperature = 3.0 } // Out of range
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.CompleteAsync(request));
    }

    #endregion

    #region EstimateCostAsync Tests

    [Fact]
    public async Task EstimateCostAsync_CalculatesCostCorrectly()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata
        {
            CostPer1KInputTokens = 0.002,
            CostPer1KOutputTokens = 0.006
        };
        var client = new TestChatClient(logger, metadata);
        var usage = new TokenUsage
        {
            InputTokens = 5000,
            OutputTokens = 3000
        };

        // Act
        var cost = await client.EstimateCostAsync(usage);

        // Assert
        cost.InputCost.Should().BeApproximately(0.01, 0.0001); // 5000 / 1000 * 0.002
        cost.OutputCost.Should().BeApproximately(0.018, 0.0001); // 3000 / 1000 * 0.006
        cost.TotalCost.Should().BeApproximately(0.028, 0.0001);
        cost.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task EstimateCostAsync_WithZeroTokens_ReturnsZeroCost()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);
        var usage = new TokenUsage();

        // Act
        var cost = await client.EstimateCostAsync(usage);

        // Assert
        cost.TotalCost.Should().Be(0);
    }

    #endregion

    #region GetTokenUsageAsync Tests

    [Fact]
    public async Task GetTokenUsageAsync_ReturnsEmptyUsage()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);

        // Act
        var usage = await client.GetTokenUsageAsync("test-id");

        // Assert
        usage.Should().NotBeNull();
        usage.InputTokens.Should().Be(0);
        usage.OutputTokens.Should().Be(0);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledSuccessfully()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);

        // Act & Assert
        client.Invoking(c => c.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata();
        var client = new TestChatClient(logger, metadata);

        // Act & Assert
        client.Invoking(c =>
        {
            c.Dispose();
            c.Dispose();
        }).Should().NotThrow();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ReturnsCorrectValues()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ChatClientBase>>();
        var metadata = new ChatClientMetadata
        {
            ModelId = "gpt-4o",
            ModelVersion = "2024-11-20",
            MaxTokens = 16384,
            ContextWindow = 128000,
            CostPer1KInputTokens = 0.0025,
            CostPer1KOutputTokens = 0.01,
            AverageLatency = TimeSpan.FromMilliseconds(850),
            AvailabilityScore = 0.999
        };
        var client = new TestChatClient(logger, metadata);

        // Assert
        client.Metadata.ModelId.Should().Be("gpt-4o");
        client.Metadata.ModelVersion.Should().Be("2024-11-20");
        client.Metadata.MaxTokens.Should().Be(16384);
        client.Metadata.ContextWindow.Should().Be(128000);
        client.Metadata.CostPer1KInputTokens.Should().Be(0.0025);
        client.Metadata.CostPer1KOutputTokens.Should().Be(0.01);
        client.Metadata.AverageLatency.Should().Be(TimeSpan.FromMilliseconds(850));
        client.Metadata.AvailabilityScore.Should().Be(0.999);
    }

    #endregion
}

/// <summary>
/// Unit tests for the <see cref="TokenUsage"/> class.
/// </summary>
public class TokenUsageTests
{
    [Fact]
    public void TotalTokens_CalculatesCorrectly()
    {
        // Arrange
        var usage = new TokenUsage
        {
            InputTokens = 100,
            OutputTokens = 50
        };

        // Assert
        usage.TotalTokens.Should().Be(150);
    }

    [Fact]
    public void TotalTokens_WithZeroValues_ReturnsZero()
    {
        // Arrange
        var usage = new TokenUsage();

        // Assert
        usage.TotalTokens.Should().Be(0);
    }

    [Fact]
    public void Breakdown_IsInitialized()
    {
        // Arrange
        var usage = new TokenUsage();

        // Assert
        usage.Breakdown.Should().NotBeNull();
        usage.Breakdown.Should().BeEmpty();
    }

    [Fact]
    public void InputTokensCacheHit_CanBeNull()
    {
        // Arrange
        var usage = new TokenUsage();

        // Assert
        usage.InputTokensCacheHit.Should().BeNull();
    }

    [Fact]
    public void InputTokensCacheHit_CanBeSet()
    {
        // Arrange
        var usage = new TokenUsage
        {
            InputTokensCacheHit = 50.0
        };

        // Assert
        usage.InputTokensCacheHit.Should().Be(50.0);
    }
}

/// <summary>
/// Unit tests for the <see cref="CostEstimate"/> class.
/// </summary>
public class CostEstimateTests
{
    [Fact]
    public void TotalCost_CalculatesCorrectly()
    {
        // Arrange
        var estimate = new CostEstimate
        {
            InputCost = 0.01,
            OutputCost = 0.02
        };

        // Assert
        estimate.TotalCost.Should().Be(0.03);
    }

    [Fact]
    public void Currency_DefaultsToUSD()
    {
        // Arrange
        var estimate = new CostEstimate();

        // Assert
        estimate.Currency.Should().Be("USD");
    }

    [Fact]
    public void Currency_CanBeSet()
    {
        // Arrange
        var estimate = new CostEstimate
        {
            Currency = "EUR"
        };

        // Assert
        estimate.Currency.Should().Be("EUR");
    }
}

/// <summary>
/// Unit tests for the <see cref="ChatClientMetadata"/> class.
/// </summary>
public class ChatClientMetadataTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange
        var metadata = new ChatClientMetadata();

        // Assert
        metadata.ModelId.Should().BeEmpty();
        metadata.ModelVersion.Should().BeEmpty();
        metadata.MaxTokens.Should().Be(0);
        metadata.ContextWindow.Should().Be(0);
        metadata.CostPer1KInputTokens.Should().Be(0);
        metadata.CostPer1KOutputTokens.Should().Be(0);
        metadata.AverageLatency.Should().Be(TimeSpan.Zero);
        metadata.AvailabilityScore.Should().Be(0);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var metadata = new ChatClientMetadata
        {
            ModelId = "gpt-4o",
            ModelVersion = "2024-11-20",
            MaxTokens = 16384,
            ContextWindow = 128000,
            CostPer1KInputTokens = 0.0025,
            CostPer1KOutputTokens = 0.01,
            AverageLatency = TimeSpan.FromMilliseconds(850),
            AvailabilityScore = 0.999
        };

        // Assert
        metadata.ModelId.Should().Be("gpt-4o");
        metadata.ModelVersion.Should().Be("2024-11-20");
        metadata.MaxTokens.Should().Be(16384);
        metadata.ContextWindow.Should().Be(128000);
        metadata.CostPer1KInputTokens.Should().Be(0.0025);
        metadata.CostPer1KOutputTokens.Should().Be(0.01);
        metadata.AverageLatency.Should().Be(TimeSpan.FromMilliseconds(850));
        metadata.AvailabilityScore.Should().Be(0.999);
    }
}
