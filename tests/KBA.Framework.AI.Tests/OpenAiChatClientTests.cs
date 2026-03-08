using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace KBA.Framework.AI.Tests;

/// <summary>
/// Unit tests for the <see cref="OpenAiChatClient"/> class.
/// </summary>
public class OpenAiChatClientTests
{
    private const string TestApiKey = "sk-test1234567890";
    private const string TestModelId = "gpt-4o";

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidOptions_CreatesClientSuccessfully()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();

        // Act
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Assert
        client.Should().NotBeNull();
        client.ProviderName.Should().Be("OpenAI");
        client.Metadata.ModelId.Should().Be(TestModelId);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        IOptions<OpenAiOptions> nullOptions = null!;
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenAiChatClient(nullOptions, logger, httpClient));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateTestOptions();
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenAiChatClient(options, null!, httpClient));
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenAiChatClient(options, logger, null!));
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions
        {
            ApiKey = null!,
            ModelId = TestModelId,
            Endpoint = "https://api.openai.com/v1",
            IsEnabled = true
        });
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();

        // Act & Assert - null API key throws ArgumentException
        Assert.Throws<ArgumentException>(() => new OpenAiChatClient(options, logger, httpClient));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions
        {
            ApiKey = string.Empty,
            ModelId = TestModelId,
            Endpoint = "https://api.openai.com/v1",
            IsEnabled = true
        });
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new OpenAiChatClient(options, logger, httpClient));
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Metadata_ReturnsCorrectModelInformation()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act
        var metadata = client.Metadata;

        // Assert
        metadata.ModelId.Should().Be(TestModelId);
        metadata.ContextWindow.Should().BeGreaterThan(0);
        metadata.MaxTokens.Should().BeGreaterThan(0);
        metadata.AvailabilityScore.Should().BeInRange(0, 1);
    }

    [Theory]
    [InlineData("gpt-4o", 0.0025, 0.010)]
    [InlineData("gpt-4-turbo", 0.01, 0.03)]
    [InlineData("gpt-4", 0.03, 0.06)]
    [InlineData("gpt-3.5-turbo", 0.0005, 0.0015)]
    public void Metadata_ReturnsCorrectPricingForModel(string modelId, double expectedInputCost, double expectedOutputCost)
    {
        // Arrange
        var options = CreateTestOptions(modelId: modelId);
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act
        var metadata = client.Metadata;

        // Assert
        metadata.CostPer1KInputTokens.Should().Be(expectedInputCost);
        metadata.CostPer1KOutputTokens.Should().Be(expectedOutputCost);
    }

    #endregion

    #region CompleteAsync Tests

    [Fact]
    public async Task CompleteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.CompleteAsync(null!));
    }

    [Fact]
    public async Task CompleteAsync_WithEmptyMessages_ThrowsArgumentException()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);
        var request = new ChatRequest { Messages = new List<ChatMessage>() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => client.CompleteAsync(request));
    }

    #endregion

    #region CompleteStreamingAsync Tests

    [Fact]
    public async Task CompleteStreamingAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.CompleteStreamingAsync(null!).ToListAsync());
    }

    [Fact]
    public async Task CompleteStreamingAsync_WithEmptyMessages_ThrowsArgumentException()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);
        var request = new ChatRequest { Messages = new List<ChatMessage>() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.CompleteStreamingAsync(request).ToListAsync());
    }

    #endregion

    #region EstimateCostAsync Tests

    [Fact]
    public async Task EstimateCostAsync_CalculatesCorrectCost()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);
        var usage = new TokenUsage
        {
            InputTokens = 1000,
            OutputTokens = 500
        };

        // Act
        var costEstimate = await client.EstimateCostAsync(usage);

        // Assert
        costEstimate.Should().NotBeNull();
        costEstimate.InputCost.Should().Be(0.0025);
        costEstimate.OutputCost.Should().Be(0.005);
        costEstimate.TotalCost.Should().Be(0.0075);
        costEstimate.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task EstimateCostAsync_WithZeroUsage_ReturnsZeroCost()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);
        var usage = new TokenUsage();

        // Act
        var costEstimate = await client.EstimateCostAsync(usage);

        // Assert
        costEstimate.TotalCost.Should().Be(0);
    }

    [Fact]
    public async Task EstimateCostAsync_WithLargeTokenCount_CalculatesCorrectly()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);
        var usage = new TokenUsage
        {
            InputTokens = 100000,
            OutputTokens = 50000
        };

        // Act
        var costEstimate = await client.EstimateCostAsync(usage);

        // Assert
        costEstimate.InputCost.Should().Be(0.25); // 100K tokens at $0.0025/1K
        costEstimate.OutputCost.Should().Be(0.5); // 50K tokens at $0.01/1K
        costEstimate.TotalCost.Should().Be(0.75);
    }

    #endregion

    #region GetTokenUsageAsync Tests

    [Fact]
    public async Task GetTokenUsageAsync_ReturnsEmptyUsage()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act
        var usage = await client.GetTokenUsageAsync("test-correlation-id");

        // Assert
        usage.Should().NotBeNull();
        usage.InputTokens.Should().Be(0);
        usage.OutputTokens.Should().Be(0);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledWithoutError()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act & Assert
        client.Invoking(c => c.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var options = CreateTestOptions();
        var logger = CreateMockLogger();
        var httpClient = new HttpClient();
        var client = new OpenAiChatClient(options, logger, httpClient);

        // Act & Assert
        client.Invoking(c =>
        {
            c.Dispose();
            c.Dispose();
            c.Dispose();
        }).Should().NotThrow();
    }

    #endregion

    #region Helper Methods

    private static IOptions<OpenAiOptions> CreateTestOptions(
        string? apiKey = null,
        string? modelId = null,
        string? endpoint = null)
    {
        var options = new OpenAiOptions
        {
            ApiKey = apiKey ?? TestApiKey,
            ModelId = modelId ?? TestModelId,
            Endpoint = endpoint ?? "https://api.openai.com/v1",
            TimeoutSeconds = 30,
            IsEnabled = true
        };

        return Options.Create(options);
    }

    private static ILogger<OpenAiChatClient> CreateMockLogger()
    {
        return Substitute.For<ILogger<OpenAiChatClient>>();
    }

    #endregion
}

/// <summary>
/// Extension methods for testing async enumerables.
/// </summary>
public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }
}
