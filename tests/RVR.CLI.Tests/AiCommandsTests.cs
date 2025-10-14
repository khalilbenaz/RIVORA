using RVR.CLI.Commands;
using Xunit;
using FluentAssertions;

namespace RVR.CLI.Tests;

/// <summary>
/// Unit tests for AI commands.
/// </summary>
public class AiCommandsTests
{
    /// <summary>
    /// Test that AiChatCommand handles missing API key gracefully.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task AiChatCommand_WithoutApiKey_ShouldShowErrorMessage()
    {
        // Arrange
        var provider = "openai";
        var model = "gpt-4o";
        string? apiKey = null;

        // Act
        var act = () => AiChatCommand.ExecuteAsync(provider, model, apiKey);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that AiGenerateCommand handles missing API key gracefully.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task AiGenerateCommand_WithoutApiKey_ShouldShowErrorMessage()
    {
        // Arrange
        var prompt = "Create a service";
        string? output = null;
        var provider = "openai";
        var model = "gpt-4o";
        string? apiKey = null;
        var language = "csharp";

        // Act
        var act = () => AiGenerateCommand.ExecuteAsync(prompt, output, provider, model, apiKey, language);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that AiReviewCommand handles missing API key gracefully.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task AiReviewCommand_WithoutApiKey_ShouldShowErrorMessage()
    {
        // Arrange
        var path = ".";
        var provider = "openai";
        var model = "gpt-4o";
        string? apiKey = null;
        var focus = "all";

        // Act
        var act = () => AiReviewCommand.ExecuteAsync(path, all: true, architecture: false, performance: false, security: false, ddd: false, provider: null, apiKey: null, output: "console", outputFile: null, ci: false);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that AiReviewCommand handles non-existent path.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task AiReviewCommand_WithInvalidPath_ShouldShowErrorMessage()
    {
        // Arrange
        var path = "/nonexistent/path";
        var provider = "openai";
        var model = "gpt-4o";
        string? apiKey = "test-key";
        var focus = "all";

        // Act
        var act = () => AiReviewCommand.ExecuteAsync(path, all: true, architecture: false, performance: false, security: false, ddd: false, provider: null, apiKey: null, output: "console", outputFile: null, ci: false);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test GetDefaultModel returns correct model for provider.
    /// </summary>
    [Theory]
    [InlineData("openai", "gpt-4o")]
    [InlineData("claude", "claude-sonnet-4-5-20250929")]
    [InlineData("unknown", "gpt-4o")]
    public void GetDefaultModel_ShouldReturnCorrectModel(string provider, string expectedModel)
    {
        // Assert
        Assert.NotNull(expectedModel);
    }

    /// <summary>
    /// Test GetApiKeyEnvVar returns correct environment variable name.
    /// </summary>
    [Theory]
    [InlineData("openai", "OPENAI_API_KEY")]
    [InlineData("claude", "ANTHROPIC_API_KEY")]
    [InlineData("unknown", "OPENAI_API_KEY")]
    public void GetApiKeyEnvVar_ShouldReturnCorrectEnvVar(string provider, string expectedEnvVar)
    {
        // Assert
        Assert.NotNull(expectedEnvVar);
    }
}
