using KBA.CLI.Commands;
using KBA.CLI.Services;
using Xunit;
using FluentAssertions;
using System.Xml.Linq;
using NSubstitute;
using Spectre.Console;

namespace KBA.CLI.Tests;

/// <summary>
/// Unit tests for UpdateCommand and related services.
/// </summary>
public class UpdateCommandTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _originalDirectory;

    public UpdateCommandTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"KBA_Update_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        Directory.SetCurrentDirectory(_testDirectory);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);
        try
        {
            Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    #region ProjectScannerService Tests

    /// <summary>
    /// Test that ProjectScannerService can find solution files.
    /// </summary>
    [Fact]
    public void ProjectScannerService_FindSolutionFile_ShouldReturnSolutionPath()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());
        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        File.WriteAllText(solutionPath, @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
");

        // Act
        var found = scanner.FindSolutionFile(_testDirectory);

        // Assert
        found.Should().NotBeNull();
        found.Should().Contain("Test.sln");
    }

    /// <summary>
    /// Test that ProjectScannerService returns null when no solution exists.
    /// </summary>
    [Fact]
    public void ProjectScannerService_FindSolutionFile_ShouldReturnNull_WhenNoSolution()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        // Act
        var found = scanner.FindSolutionFile(_testDirectory);

        // Assert
        found.Should().BeNull();
    }

    /// <summary>
    /// Test that ProjectScannerService can parse solution file and find project files.
    /// </summary>
    [Fact]
    public void ProjectScannerService_GetProjectFilesFromSolution_ShouldReturnProjects()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        var projectDir = Path.Combine(_testDirectory, "src", "MyProject");
        Directory.CreateDirectory(projectDir);
        var projectPath = Path.Combine(projectDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        // Use proper solution file format with escaped backslashes
        File.WriteAllText(solutionPath, $@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""MyProject"", ""src\\MyProject\\MyProject.csproj"", ""{{12345678-1234-1234-1234-123456789012}}""
EndProject
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
    EndGlobalSection
EndGlobal
");

        // Act
        var projects = scanner.GetProjectFilesFromSolution(solutionPath).ToList();

        // Assert
        projects.Should().HaveCount(1);
        projects.First().Should().Contain("MyProject.csproj");
    }

    /// <summary>
    /// Test that ProjectScannerService can extract package references from project file.
    /// </summary>
    [Fact]
    public void ProjectScannerService_GetPackageReferences_ShouldExtractReferences()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var projectPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(projectPath, @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""KBA.Framework.Domain"" Version=""1.0.0"" />
    <PackageReference Include=""KBA.Framework.Application"" Version=""2.0.0"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.0"" />
  </ItemGroup>
</Project>");

        // Act
        var references = scanner.GetPackageReferences(projectPath).ToList();

        // Assert
        references.Should().HaveCount(3);
        references.Should().Contain(r => r.PackageName == "KBA.Framework.Domain" && r.Version == "1.0.0");
        references.Should().Contain(r => r.PackageName == "KBA.Framework.Application" && r.Version == "2.0.0");
    }

    /// <summary>
    /// Test that ProjectScannerService can filter KBA packages.
    /// </summary>
    [Fact]
    public void ProjectScannerService_FilterKbaPackages_ShouldFilterCorrectly()
    {
        // Arrange
        var references = new List<PackageReferenceInfo>
        {
            new() { PackageName = "KBA.Framework.Domain", Version = "1.0.0" },
            new() { PackageName = "KBA.Framework.Application", Version = "2.0.0" },
            new() { PackageName = "Newtonsoft.Json", Version = "13.0.0" },
            new() { PackageName = "KBA.CLI", Version = "1.0.0" }
        };

        // Act
        var filtered = ProjectScannerService.FilterKbaPackages(references).ToList();

        // Assert
        filtered.Should().HaveCount(3);
        filtered.Should().NotContain(r => r.PackageName == "Newtonsoft.Json");
    }

    /// <summary>
    /// Test that ProjectScannerService can detect central package management.
    /// </summary>
    [Fact]
    public void ProjectScannerService_UsesCentralPackageManagement_ShouldReturnTrue_WhenEnabled()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        var packagesPropsPath = Path.Combine(_testDirectory, "Directory.Packages.props");

        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");
        File.WriteAllText(packagesPropsPath, @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>");

        // Act
        var usesCentral = scanner.UsesCentralPackageManagement(solutionPath);

        // Assert
        usesCentral.Should().BeTrue();
    }

    /// <summary>
    /// Test that ProjectScannerService returns false when central management is disabled.
    /// </summary>
    [Fact]
    public void ProjectScannerService_UsesCentralPackageManagement_ShouldReturnFalse_WhenDisabled()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");

        // Act
        var usesCentral = scanner.UsesCentralPackageManagement(solutionPath);

        // Assert
        usesCentral.Should().BeFalse();
    }

    /// <summary>
    /// Test that ProjectScannerService can get central package versions.
    /// </summary>
    [Fact]
    public void ProjectScannerService_GetCentralPackageVersions_ShouldReturnVersions()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        var packagesPropsPath = Path.Combine(_testDirectory, "Directory.Packages.props");

        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");
        File.WriteAllText(packagesPropsPath, @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include=""KBA.Framework.Domain"" Version=""1.0.0"" />
    <PackageVersion Include=""KBA.Framework.Application"" Version=""2.0.0"" />
  </ItemGroup>
</Project>");

        // Act
        var versions = scanner.GetCentralPackageVersions(solutionPath);

        // Assert
        versions.Should().ContainKey("KBA.Framework.Domain").WhoseValue.Should().Be("1.0.0");
        versions.Should().ContainKey("KBA.Framework.Application").WhoseValue.Should().Be("2.0.0");
    }

    /// <summary>
    /// Test that ProjectScannerService can update central package version.
    /// </summary>
    [Fact]
    public void ProjectScannerService_UpdateCentralPackageVersion_ShouldUpdateVersion()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        var packagesPropsPath = Path.Combine(_testDirectory, "Directory.Packages.props");

        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");
        File.WriteAllText(packagesPropsPath, @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include=""KBA.Framework.Domain"" Version=""1.0.0"" />
  </ItemGroup>
</Project>");

        // Act
        var success = scanner.UpdateCentralPackageVersion(solutionPath, "KBA.Framework.Domain", "2.0.0");

        // Assert
        success.Should().BeTrue();

        var content = File.ReadAllText(packagesPropsPath);
        content.Should().Contain("Version=\"2.0.0\"");
    }

    /// <summary>
    /// Test that ProjectScannerService can update project package version.
    /// </summary>
    [Fact]
    public void ProjectScannerService_UpdateProjectPackageVersion_ShouldUpdateVersion()
    {
        // Arrange
        var scanner = new ProjectScannerService(Substitute.For<IAnsiConsole>());

        var projectPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(projectPath, @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""KBA.Framework.Domain"" Version=""1.0.0"" />
  </ItemGroup>
</Project>");

        // Act
        var success = scanner.UpdateProjectPackageVersion(projectPath, "KBA.Framework.Domain", "2.0.0");

        // Assert
        success.Should().BeTrue();

        var content = File.ReadAllText(projectPath);
        content.Should().Contain("Version=\"2.0.0\"");
    }

    #endregion

    #region NuGetService Tests

    /// <summary>
    /// Test that NuGetService can be instantiated.
    /// </summary>
    [Fact]
    public void NuGetService_Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var console = Substitute.For<IAnsiConsole>();
        var service = new NuGetService(console);

        // Assert
        service.Should().NotBeNull();
    }

    /// <summary>
    /// Test that SemanticVersion can parse stable versions.
    /// </summary>
    [Fact]
    public void SemanticVersion_Parse_ShouldParseStableVersion()
    {
        // Act
        var version = SemanticVersion.Parse("1.2.3");

        // Assert
        version.Version.Should().Be(new Version(1, 2, 3, 0));
        version.PrereleaseLabel.Should().BeEmpty();
    }

    /// <summary>
    /// Test that SemanticVersion can parse preview versions.
    /// </summary>
    [Fact]
    public void SemanticVersion_Parse_ShouldParsePreviewVersion()
    {
        // Act
        var version = SemanticVersion.Parse("1.2.3-preview.1");

        // Assert
        version.Version.Should().Be(new Version(1, 2, 3, 0));
        version.PrereleaseLabel.Should().Be("preview.1");
    }

    /// <summary>
    /// Test that SemanticVersion can parse beta versions.
    /// </summary>
    [Fact]
    public void SemanticVersion_Parse_ShouldParseBetaVersion()
    {
        // Act
        var version = SemanticVersion.Parse("2.0.0-beta.5");

        // Assert
        version.Version.Should().Be(new Version(2, 0, 0, 0));
        version.PrereleaseLabel.Should().Be("beta.5");
    }

    /// <summary>
    /// Test that SemanticVersion compares stable versions correctly.
    /// </summary>
    [Fact]
    public void SemanticVersion_Compare_ShouldCompareStableVersions()
    {
        // Arrange
        var v1 = SemanticVersion.Parse("1.0.0");
        var v2 = SemanticVersion.Parse("2.0.0");
        var v3 = SemanticVersion.Parse("1.5.0");

        // Assert
        v1.CompareTo(v2).Should().BeNegative();
        v2.CompareTo(v1).Should().BePositive();
        v3.CompareTo(v1).Should().BePositive();
        v3.CompareTo(v2).Should().BeNegative();
    }

    /// <summary>
    /// Test that SemanticVersion compares prerelease versions correctly.
    /// </summary>
    [Fact]
    public void SemanticVersion_Compare_ShouldComparePrereleaseVersions()
    {
        // Arrange
        var stable = SemanticVersion.Parse("1.0.0");
        var preview = SemanticVersion.Parse("1.0.0-preview.1");
        var beta = SemanticVersion.Parse("1.0.0-beta.1");
        var alpha = SemanticVersion.Parse("1.0.0-alpha.1");

        // Assert - stable should be greater than prerelease
        stable.CompareTo(preview).Should().BePositive();
        preview.CompareTo(stable).Should().BeNegative();

        // beta should be greater than alpha (beta comes after alpha in release cycle)
        beta.CompareTo(alpha).Should().BePositive();
        alpha.CompareTo(beta).Should().BeNegative();

        // preview should be greater than beta (preview comes after beta in release cycle)
        preview.CompareTo(beta).Should().BePositive();
    }

    #endregion

    #region UpdateCommand Execution Tests

    /// <summary>
    /// Test that UpdateCommand executes without throwing when no solution exists.
    /// </summary>
    [Fact]
    public async Task UpdateCommand_ExecuteAsync_ShouldNotThrow_WhenNoSolution()
    {
        // Arrange & Act
        var act = () => UpdateCommand.ExecuteAsync(
            preview: false,
            nightly: false,
            dryRun: true,
            verbose: false);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that UpdateCommand executes with dry-run mode.
    /// </summary>
    [Fact]
    public async Task UpdateCommand_ExecuteAsync_WithDryRun_ShouldNotThrow()
    {
        // Arrange - Create a minimal solution
        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");

        // Act
        var act = () => UpdateCommand.ExecuteAsync(
            preview: false,
            nightly: false,
            dryRun: true,
            verbose: false);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that UpdateCommand executes with preview option.
    /// </summary>
    [Fact]
    public async Task UpdateCommand_ExecuteAsync_WithPreview_ShouldNotThrow()
    {
        // Arrange
        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");

        // Act
        var act = () => UpdateCommand.ExecuteAsync(
            preview: true,
            nightly: false,
            dryRun: true,
            verbose: false);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that UpdateCommand executes with nightly option.
    /// </summary>
    [Fact]
    public async Task UpdateCommand_ExecuteAsync_WithNightly_ShouldNotThrow()
    {
        // Arrange
        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");

        // Act
        var act = () => UpdateCommand.ExecuteAsync(
            preview: true,
            nightly: true,
            dryRun: true,
            verbose: false);

        // Assert
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Test that UpdateCommand executes with verbose option.
    /// </summary>
    [Fact]
    public async Task UpdateCommand_ExecuteAsync_WithVerbose_ShouldNotThrow()
    {
        // Arrange
        var solutionPath = Path.Combine(_testDirectory, "Test.sln");
        File.WriteAllText(solutionPath, "Microsoft Visual Studio Solution File");

        // Act
        var act = () => UpdateCommand.ExecuteAsync(
            preview: false,
            nightly: false,
            dryRun: true,
            verbose: true);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region PackageUpdateInfo Tests

    /// <summary>
    /// Test that PackageUpdateInfo initializes with default values.
    /// </summary>
    [Fact]
    public void PackageUpdateInfo_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var info = new PackageUpdateInfo();

        // Assert
        info.PackageName.Should().BeEmpty();
        info.CurrentVersion.Should().BeEmpty();
        info.LatestVersion.Should().BeEmpty();
        info.DownloadUrl.Should().BeEmpty();
        info.SourceFiles.Should().BeEmpty();
        info.IsPreview.Should().BeFalse();
        info.IsNightly.Should().BeFalse();
        info.IsCentrallyManaged.Should().BeFalse();
    }

    #endregion

    #region PackageReferenceInfo Tests

    /// <summary>
    /// Test that PackageReferenceInfo initializes with default values.
    /// </summary>
    [Fact]
    public void PackageReferenceInfo_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var info = new PackageReferenceInfo();

        // Assert
        info.PackageName.Should().BeEmpty();
        info.Version.Should().BeNull();
        info.SourceFile.Should().BeEmpty();
        info.IsCentrallyManaged.Should().BeFalse();
    }

    #endregion
}
