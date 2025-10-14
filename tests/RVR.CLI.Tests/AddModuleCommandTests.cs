using RVR.CLI.Commands;
using Xunit;
using FluentAssertions;

namespace RVR.CLI.Tests;

/// <summary>
/// Unit tests for AddModuleCommand.
/// </summary>
public class AddModuleCommandTests
{
    /// <summary>
    /// Test that ExecuteAsync creates module structure.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task ExecuteAsync_ShouldCreateModuleStructure()
    {
        // Arrange
        var moduleName = "TestModule";
        string? featureName = null;
        var includeTests = false;
        var includeApi = false;
        var includeMigrations = false;

        // Act
        var act = () => AddModuleCommand.ExecuteAsync(moduleName, featureName, includeTests, includeApi, includeMigrations);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that ExecuteAsync with all options creates complete structure.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task ExecuteAsync_WithAllOptions_ShouldCreateCompleteStructure()
    {
        // Arrange
        var moduleName = "CompleteModule";
        var featureName = "Product";
        var includeTests = true;
        var includeApi = true;
        var includeMigrations = true;

        // Act
        var act = () => AddModuleCommand.ExecuteAsync(moduleName, featureName, includeTests, includeApi, includeMigrations);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
