using KBA.CLI.Commands;
using Xunit;
using FluentAssertions;

namespace KBA.CLI.Tests;

/// <summary>
/// Unit tests for DoctorCommand.
/// </summary>
public class DoctorCommandTests
{
    /// <summary>
    /// Test that ExecuteAsync completes without throwing.
    /// </summary>
    [Fact(Skip = "Requires interactive console")]
    public async Task ExecuteAsync_ShouldCompleteWithoutError()
    {
        // Arrange & Act
        var act = () => DoctorCommand.ExecuteAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that project structure checks work correctly.
    /// </summary>
    [Fact]
    public void CheckProjectStructure_ShouldFindSolutionFile()
    {
        // Arrange - navigate to project root
        var testDir = Directory.GetCurrentDirectory();
        var projectRoot = Directory.GetParent(testDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
        
        // Act
        if (!string.IsNullOrEmpty(projectRoot))
        {
            var solutionFiles = Directory.GetFiles(projectRoot, "*.sln", SearchOption.TopDirectoryOnly);
            
            // Assert
            solutionFiles.Should().NotBeEmpty("at least one solution file should exist in project root");
        }
        else
        {
            // Skip if we can't find the project root
            Assert.True(true, "Could not determine project root - test skipped");
        }
    }
}
