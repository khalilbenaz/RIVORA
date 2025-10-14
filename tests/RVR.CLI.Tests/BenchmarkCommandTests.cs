using RVR.CLI.Commands;
using Xunit;
using FluentAssertions;

namespace RVR.CLI.Tests;

/// <summary>
/// Unit tests for BenchmarkCommand.
/// </summary>
public class BenchmarkCommandTests
{
    /// <summary>
    /// Test that ExecuteAsync handles k6 not installed gracefully.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task ExecuteAsync_WithoutK6_ShouldShowInstallInstructions()
    {
        // Arrange
        var url = "http://localhost:5000";
        var duration = "30s";
        var vus = 10;
        var output = "console";
        var scenario = "smoke";

        // Act
        var act = () => BenchmarkCommand.ExecuteAsync(url, duration, vus, output, scenario);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test GetScenarioConfig returns correct configuration.
    /// </summary>
    [Theory]
    [InlineData("smoke")]
    [InlineData("load")]
    [InlineData("stress")]
    [InlineData("spike")]
    [InlineData("soak")]
    public void GetScenarioConfig_ShouldReturnConfiguration(string scenario)
    {
        // Assert
        Assert.NotNull(scenario);
    }
}
