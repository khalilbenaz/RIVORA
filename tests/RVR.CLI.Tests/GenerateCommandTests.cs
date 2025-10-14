using RVR.CLI.Commands;
using Xunit;
using FluentAssertions;

namespace RVR.CLI.Tests;

/// <summary>
/// Unit tests for GenerateCommand.
/// </summary>
public class GenerateCommandTests
{
    /// <summary>
    /// Test that GenerateAggregateAsync completes without error.
    /// </summary>
    [Fact]
    public async Task GenerateAggregateAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var name = "Product";
        var module = "Catalog";

        // Act
        var act = () => GenerateCommand.GenerateAggregateAsync(name, module);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that GenerateCrudAsync completes without error.
    /// </summary>
    [Fact]
    public async Task GenerateCrudAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var name = "Order";
        var props = "Name:string,Price:decimal";

        // Act
        var act = () => GenerateCommand.GenerateCrudAsync(name, props);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that GenerateCommandAsync completes without error.
    /// </summary>
    [Fact]
    public async Task GenerateCommandAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var name = "CreateProduct";

        // Act
        var act = () => GenerateCommand.GenerateCommandAsync(name);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that GenerateQueryAsync completes without error.
    /// </summary>
    [Fact]
    public async Task GenerateQueryAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var name = "GetProductById";

        // Act
        var act = () => GenerateCommand.GenerateQueryAsync(name);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
