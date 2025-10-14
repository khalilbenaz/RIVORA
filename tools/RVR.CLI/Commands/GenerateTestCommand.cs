using System.Text;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Generates xUnit + FluentAssertions unit tests for a given entity.
/// Usage: rvr generate test &lt;EntityName&gt; [--output &lt;dir&gt;]
/// </summary>
public static class GenerateTestCommand
{
    /// <summary>
    /// Executes the test generation command.
    /// </summary>
    /// <param name="entityName">Name of the entity to generate tests for.</param>
    /// <param name="outputDir">Output directory (default: tests/).</param>
    public static async Task ExecuteAsync(string entityName, string outputDir)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating unit tests for entity: {0}[/]", entityName);

        var dir = string.IsNullOrWhiteSpace(outputDir) ? "tests" : outputDir;
        var testDir = Path.Combine(dir, $"{entityName}Tests");
        Directory.CreateDirectory(testDir);

        // Generate constructor tests
        var constructorTests = GenerateConstructorTests(entityName);
        var constructorTestPath = Path.Combine(testDir, $"{entityName}ConstructorTests.cs");
        await File.WriteAllTextAsync(constructorTestPath, constructorTests);
        AnsiConsole.MarkupLine("[green]  + Created[/] {0}", constructorTestPath);

        // Generate property tests
        var propertyTests = GeneratePropertyTests(entityName);
        var propertyTestPath = Path.Combine(testDir, $"{entityName}PropertyTests.cs");
        await File.WriteAllTextAsync(propertyTestPath, propertyTests);
        AnsiConsole.MarkupLine("[green]  + Created[/] {0}", propertyTestPath);

        // Generate method tests
        var methodTests = GenerateMethodTests(entityName);
        var methodTestPath = Path.Combine(testDir, $"{entityName}MethodTests.cs");
        await File.WriteAllTextAsync(methodTestPath, methodTests);
        AnsiConsole.MarkupLine("[green]  + Created[/] {0}", methodTestPath);

        AnsiConsole.MarkupLine("[bold green]Test generation complete![/] 3 test files created in [blue]{0}[/]", testDir);
    }

    private static string GenerateConstructorTests(string entityName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using FluentAssertions;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Tests.{entityName}Tests;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Constructor tests for <see cref=\"{entityName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entityName}ConstructorTests");
        sb.AppendLine("{");
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void Constructor_ShouldCreateInstance()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange & Act");
        sb.AppendLine($"        var sut = new {entityName}();");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        sut.Should().NotBeNull();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void Constructor_ShouldInitializeDefaultValues()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange & Act");
        sb.AppendLine($"        var sut = new {entityName}();");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        sut.Id.Should().BeEmpty();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void Constructor_WithId_ShouldSetId()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var id = Guid.NewGuid();");
        sb.AppendLine();
        sb.AppendLine($"        // Act");
        sb.AppendLine($"        var sut = new {entityName} {{ Id = id }};");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        sut.Id.Should().Be(id);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GeneratePropertyTests(string entityName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using FluentAssertions;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Tests.{entityName}Tests;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Property tests for <see cref=\"{entityName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entityName}PropertyTests");
        sb.AppendLine("{");
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void Id_ShouldBeSettable()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var sut = new {entityName}();");
        sb.AppendLine($"        var id = Guid.NewGuid();");
        sb.AppendLine();
        sb.AppendLine($"        // Act");
        sb.AppendLine($"        sut.Id = id;");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        sut.Id.Should().Be(id);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void CreatedAt_ShouldBeSettable()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var sut = new {entityName}();");
        sb.AppendLine($"        var date = DateTime.UtcNow;");
        sb.AppendLine();
        sb.AppendLine($"        // Act");
        sb.AppendLine($"        sut.CreatedAt = date;");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        sut.CreatedAt.Should().Be(date);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void UpdatedAt_ShouldBeNullableAndSettable()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var sut = new {entityName}();");
        sb.AppendLine();
        sb.AppendLine($"        // Assert - default is null");
        sb.AppendLine($"        sut.UpdatedAt.Should().BeNull();");
        sb.AppendLine();
        sb.AppendLine($"        // Act");
        sb.AppendLine($"        var date = DateTime.UtcNow;");
        sb.AppendLine($"        sut.UpdatedAt = date;");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        sut.UpdatedAt.Should().Be(date);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateMethodTests(string entityName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using FluentAssertions;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine();
        sb.AppendLine($"namespace RVR.Framework.Tests.{entityName}Tests;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Method tests for <see cref=\"{entityName}\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {entityName}MethodTests");
        sb.AppendLine("{");
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void ToString_ShouldReturnMeaningfulString()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var sut = new {entityName} {{ Id = Guid.NewGuid() }};");
        sb.AppendLine();
        sb.AppendLine($"        // Act");
        sb.AppendLine($"        var result = sut.ToString();");
        sb.AppendLine();
        sb.AppendLine($"        // Assert");
        sb.AppendLine($"        result.Should().NotBeNullOrEmpty();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void Equals_SameId_ShouldBeEqual()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var id = Guid.NewGuid();");
        sb.AppendLine($"        var sut1 = new {entityName} {{ Id = id }};");
        sb.AppendLine($"        var sut2 = new {entityName} {{ Id = id }};");
        sb.AppendLine();
        sb.AppendLine($"        // Act & Assert");
        sb.AppendLine($"        sut1.Id.Should().Be(sut2.Id);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [Fact]");
        sb.AppendLine($"    public void Equals_DifferentId_ShouldNotBeEqual()");
        sb.AppendLine("    {");
        sb.AppendLine($"        // Arrange");
        sb.AppendLine($"        var sut1 = new {entityName} {{ Id = Guid.NewGuid() }};");
        sb.AppendLine($"        var sut2 = new {entityName} {{ Id = Guid.NewGuid() }};");
        sb.AppendLine();
        sb.AppendLine($"        // Act & Assert");
        sb.AppendLine($"        sut1.Id.Should().NotBe(sut2.Id);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
