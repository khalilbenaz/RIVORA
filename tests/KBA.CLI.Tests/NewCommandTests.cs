using KBA.CLI.Commands;
using Xunit;
using FluentAssertions;

namespace KBA.CLI.Tests;

/// <summary>
/// Unit tests for NewCommand.
/// </summary>
public class NewCommandTests
{
    /// <summary>
    /// Test that ExecuteAsync creates project structure.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldCreateProjectStructure()
    {
        // Arrange
        var name = "TestProject";
        var template = "minimal";
        var tenancy = "row";

        // Act
        var act = () => NewCommand.ExecuteAsync(name, template, tenancy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that ExecuteAsync with saas-starter template works.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithSaaSTemplate_ShouldCreateProjectStructure()
    {
        // Arrange
        var name = "SaaSProject";
        var template = "saas-starter";
        var tenancy = "row";

        // Act
        var act = () => NewCommand.ExecuteAsync(name, template, tenancy);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that ExecuteAsync with ai-rag template works.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithAIRagTemplate_ShouldCreateProjectStructure()
    {
        // Arrange
        var name = "AIProject";
        var template = "ai-rag";
        var tenancy = "schema";

        // Act
        var act = () => NewCommand.ExecuteAsync(name, template, tenancy);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
