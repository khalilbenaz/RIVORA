using System.Text.RegularExpressions;
using System.Xml.Linq;
using Spectre.Console;

namespace KBA.CLI.Services;

/// <summary>
/// Provides services for scanning and analyzing .NET project files.
/// </summary>
public class ProjectScannerService
{
    private readonly IAnsiConsole _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectScannerService"/> class.
    /// </summary>
    /// <param name="console">The ANSI console for output.</param>
    public ProjectScannerService(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Finds the solution file in the current or specified directory.
    /// </summary>
    /// <param name="searchPath">The path to search for a solution file.</param>
    /// <returns>The solution file path, or null if not found.</returns>
    public string? FindSolutionFile(string searchPath = ".")
    {
        var solutionFiles = Directory.GetFiles(searchPath, "*.sln", SearchOption.TopDirectoryOnly);
        return solutionFiles.FirstOrDefault();
    }

    /// <summary>
    /// Gets all project files referenced by a solution file.
    /// </summary>
    /// <param name="solutionPath">The path to the solution file.</param>
    /// <returns>A collection of project file paths.</returns>
    public IEnumerable<string> GetProjectFilesFromSolution(string solutionPath)
    {
        var projectFiles = new List<string>();
        var solutionDir = Path.GetDirectoryName(solutionPath) ?? ".";

        try
        {
            var solutionContent = File.ReadAllText(solutionPath);
            // Match solution file project entries with both forward and backward slashes
            var projectPattern = new Regex(@"Project\([^)]+\)\s*=\s*""[^""]+"",\s*""(?<path>[^""]+\.csproj)"",\s*""[^""]+""");

            foreach (Match match in projectPattern.Matches(solutionContent))
            {
                if (match.Success)
                {
                    var relativePath = match.Groups["path"].Value.Replace('\\', Path.DirectorySeparatorChar);
                    var fullPath = Path.GetFullPath(Path.Combine(solutionDir, relativePath));
                    if (File.Exists(fullPath))
                    {
                        projectFiles.Add(fullPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[yellow]Warning: Error parsing solution file: {ex.Message}[/]");
        }

        return projectFiles;
    }

    /// <summary>
    /// Gets all project files in a directory tree.
    /// </summary>
    /// <param name="searchPath">The path to search for project files.</param>
    /// <returns>A collection of project file paths.</returns>
    public IEnumerable<string> GetAllProjectFiles(string searchPath = ".")
    {
        try
        {
            return Directory.GetFiles(searchPath, "*.csproj", SearchOption.AllDirectories);
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[yellow]Warning: Error finding project files: {ex.Message}[/]");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Extracts all package references from a project file.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <returns>A collection of package references with their versions.</returns>
    public IEnumerable<PackageReferenceInfo> GetPackageReferences(string projectPath)
    {
        var references = new List<PackageReferenceInfo>();

        try
        {
            var doc = XDocument.Load(projectPath);
            var projectDir = Path.GetDirectoryName(projectPath) ?? ".";

            // Find all ItemGroup elements
            foreach (var itemGroup in doc.Descendants("ItemGroup"))
            {
                // Find PackageReference elements
                foreach (var packageRef in itemGroup.Elements("PackageReference"))
                {
                    var includeAttr = packageRef.Attribute("Include");
                    if (includeAttr == null) continue;

                    var packageName = includeAttr.Value;
                    var version = packageRef.Attribute("Version")?.Value;

                    // Check for VersionOverride (for central package management)
                    var versionOverrideAttr = packageRef.Attribute("VersionOverride");
                    if (versionOverrideAttr != null)
                    {
                        version = versionOverrideAttr.Value;
                    }

                    // Check if version comes from central management
                    var isCentrallyManaged = string.IsNullOrEmpty(version) || version.StartsWith("$(");

                    if (isCentrallyManaged)
                    {
                        // Try to resolve from Directory.Packages.props
                        version = ResolveCentralVersion(projectDir, packageName);
                    }

                    if (!string.IsNullOrEmpty(packageName))
                    {
                        references.Add(new PackageReferenceInfo
                        {
                            PackageName = packageName,
                            Version = version,
                            SourceFile = projectPath,
                            IsCentrallyManaged = isCentrallyManaged
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[yellow]Warning: Error parsing project file {projectPath}: {ex.Message}[/]");
        }

        return references;
    }

    /// <summary>
    /// Checks if a solution uses central package management.
    /// </summary>
    /// <param name="solutionPath">The path to the solution file.</param>
    /// <returns>True if central package management is used.</returns>
    public bool UsesCentralPackageManagement(string solutionPath)
    {
        var solutionDir = Path.GetDirectoryName(solutionPath) ?? ".";
        var directoryPackagesProps = Path.Combine(solutionDir, "Directory.Packages.props");

        if (!File.Exists(directoryPackagesProps))
        {
            // Also check parent directories
            var parentDir = Directory.GetParent(solutionDir);
            while (parentDir != null)
            {
                directoryPackagesProps = Path.Combine(parentDir.FullName, "Directory.Packages.props");
                if (File.Exists(directoryPackagesProps))
                {
                    return true;
                }
                parentDir = parentDir.Parent;
            }
            return false;
        }

        try
        {
            var doc = XDocument.Load(directoryPackagesProps);
            var manageAttr = doc.Descendants("PropertyGroup")
                .SelectMany(pg => pg.Elements("ManagePackageVersionsCentrally"))
                .FirstOrDefault();

            return manageAttr?.Value?.ToLower() == "true";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the central package versions from Directory.Packages.props.
    /// </summary>
    /// <param name="solutionPath">The path to the solution file.</param>
    /// <returns>A dictionary of package names to versions.</returns>
    public Dictionary<string, string> GetCentralPackageVersions(string solutionPath)
    {
        var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var solutionDir = Path.GetDirectoryName(solutionPath) ?? ".";

        // Find Directory.Packages.props
        var directoryPackagesProps = FindDirectoryPackagesProps(solutionDir);
        if (directoryPackagesProps == null)
        {
            return versions;
        }

        try
        {
            var doc = XDocument.Load(directoryPackagesProps);

            foreach (var itemGroup in doc.Descendants("ItemGroup"))
            {
                // Check for PackageVersion elements
                foreach (var packageVersion in itemGroup.Elements("PackageVersion"))
                {
                    var includeAttr = packageVersion.Attribute("Include");
                    var versionAttr = packageVersion.Attribute("Version");

                    if (includeAttr != null && versionAttr != null)
                    {
                        versions[includeAttr.Value] = versionAttr.Value;
                    }
                }

                // Check for PackageReference elements with Version
                foreach (var packageRef in itemGroup.Elements("PackageReference"))
                {
                    var includeAttr = packageRef.Attribute("Include");
                    var versionAttr = packageRef.Attribute("Version");

                    if (includeAttr != null && versionAttr != null)
                    {
                        versions[includeAttr.Value] = versionAttr.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[yellow]Warning: Error parsing Directory.Packages.props: {ex.Message}[/]");
        }

        return versions;
    }

    /// <summary>
    /// Updates a package version in Directory.Packages.props.
    /// </summary>
    /// <param name="solutionPath">The path to the solution file.</param>
    /// <param name="packageName">The name of the package to update.</param>
    /// <param name="newVersion">The new version to set.</param>
    /// <returns>True if the update was successful.</returns>
    public bool UpdateCentralPackageVersion(string solutionPath, string packageName, string newVersion)
    {
        var solutionDir = Path.GetDirectoryName(solutionPath) ?? ".";
        var directoryPackagesProps = FindDirectoryPackagesProps(solutionDir);

        if (directoryPackagesProps == null)
        {
            _console.MarkupLine($"[yellow]Warning: Directory.Packages.props not found.[/]");
            return false;
        }

        try
        {
            var doc = XDocument.Load(directoryPackagesProps);
            var updated = false;

            // Update PackageVersion elements
            foreach (var packageVersion in doc.Descendants("PackageVersion"))
            {
                var includeAttr = packageVersion.Attribute("Include");
                if (includeAttr?.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var versionAttr = packageVersion.Attribute("Version");
                    if (versionAttr != null)
                    {
                        versionAttr.Value = newVersion;
                        updated = true;
                    }
                }
            }

            // Update PackageReference elements
            foreach (var packageRef in doc.Descendants("PackageReference"))
            {
                var includeAttr = packageRef.Attribute("Include");
                if (includeAttr?.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var versionAttr = packageRef.Attribute("Version");
                    if (versionAttr != null)
                    {
                        versionAttr.Value = newVersion;
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                doc.Save(directoryPackagesProps);
                return true;
            }

            _console.MarkupLine($"[yellow]Warning: Package {packageName} not found in Directory.Packages.props.[/]");
            return false;
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error: Failed to update Directory.Packages.props: {ex.Message}[/]");
            return false;
        }
    }

    /// <summary>
    /// Updates a package version in a project file.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <param name="packageName">The name of the package to update.</param>
    /// <param name="newVersion">The new version to set.</param>
    /// <returns>True if the update was successful.</returns>
    public bool UpdateProjectPackageVersion(string projectPath, string packageName, string newVersion)
    {
        try
        {
            var doc = XDocument.Load(projectPath);
            var updated = false;

            foreach (var itemGroup in doc.Descendants("ItemGroup"))
            {
                foreach (var packageRef in itemGroup.Elements("PackageReference"))
                {
                    var includeAttr = packageRef.Attribute("Include");
                    if (includeAttr?.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Check for VersionOverride first
                        var versionOverrideAttr = packageRef.Attribute("VersionOverride");
                        if (versionOverrideAttr != null)
                        {
                            versionOverrideAttr.Value = newVersion;
                            updated = true;
                        }
                        else
                        {
                            // Update or add Version attribute
                            var versionAttr = packageRef.Attribute("Version");
                            if (versionAttr != null)
                            {
                                versionAttr.Value = newVersion;
                            }
                            else
                            {
                                packageRef.Add(new XAttribute("Version", newVersion));
                            }
                            updated = true;
                        }
                    }
                }
            }

            if (updated)
            {
                doc.Save(projectPath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[red]Error: Failed to update project file {projectPath}: {ex.Message}[/]");
            return false;
        }
    }

    /// <summary>
    /// Filters package references to only KBA.Framework packages.
    /// </summary>
    /// <param name="references">The collection of package references.</param>
    /// <returns>Filtered collection of KBA.Framework packages.</returns>
    public static IEnumerable<PackageReferenceInfo> FilterKbaPackages(IEnumerable<PackageReferenceInfo> references)
    {
        return references.Where(r =>
            r.PackageName.StartsWith("KBA.Framework", StringComparison.OrdinalIgnoreCase) ||
            r.PackageName.StartsWith("KBA.CLI", StringComparison.OrdinalIgnoreCase) ||
            r.PackageName.StartsWith("KBA.Core", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolves a central package version from Directory.Packages.props.
    /// </summary>
    private string? ResolveCentralVersion(string projectDir, string packageName)
    {
        var directoryPackagesProps = FindDirectoryPackagesProps(projectDir);
        if (directoryPackagesProps == null)
        {
            return null;
        }

        try
        {
            var doc = XDocument.Load(directoryPackagesProps);

            foreach (var itemGroup in doc.Descendants("ItemGroup"))
            {
                foreach (var packageVersion in itemGroup.Elements("PackageVersion"))
                {
                    var includeAttr = packageVersion.Attribute("Include");
                    var versionAttr = packageVersion.Attribute("Version");

                    if (includeAttr?.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase) == true &&
                        versionAttr != null)
                    {
                        return versionAttr.Value;
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }

    /// <summary>
    /// Finds Directory.Packages.props in the directory tree.
    /// </summary>
    private string? FindDirectoryPackagesProps(string startDir)
    {
        var currentDir = startDir;

        while (!string.IsNullOrEmpty(currentDir))
        {
            var path = Path.Combine(currentDir, "Directory.Packages.props");
            if (File.Exists(path))
            {
                return path;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return null;
    }
}

/// <summary>
/// Represents information about a package reference.
/// </summary>
public class PackageReferenceInfo
{
    /// <summary>
    /// Gets or sets the package name.
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the source file path.
    /// </summary>
    public string SourceFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the version is centrally managed.
    /// </summary>
    public bool IsCentrallyManaged { get; set; }
}
