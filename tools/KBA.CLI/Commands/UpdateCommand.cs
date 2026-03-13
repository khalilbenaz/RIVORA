using System.Diagnostics;
using KBA.CLI.Services;
using Spectre.Console;

namespace KBA.CLI.Commands;

/// <summary>
/// Provides functionality to check and update KBA.Framework NuGet packages.
/// </summary>
/// <remarks>
/// <para>
/// The <c>update</c> command scans the solution for KBA.Framework packages and checks
/// for available updates on NuGet.org. It supports both stable and preview channels,
/// central package management, and dry-run mode for testing updates before applying them.
/// </para>
/// <para>
/// <strong>Examples:</strong>
/// <code>
/// # Check for updates (stable channel only)
/// kba update
/// 
/// # Check for updates including preview versions
/// kba update --preview
/// 
/// # Check for updates including nightly builds
/// kba update --nightly
/// 
/// # Preview updates without applying them
/// kba update --dry-run
/// 
/// # Update including preview versions
/// kba update --preview --dry-run
/// 
/// # Update all packages including nightly builds
/// kba update --nightly
/// </code>
/// </para>
/// </remarks>
public static class UpdateCommand
{
    /// <summary>
    /// Executes the update command to check and apply package updates.
    /// </summary>
    /// <param name="preview">Include preview versions in update check.</param>
    /// <param name="nightly">Include nightly builds in update check.</param>
    /// <param name="dryRun">Show what would be updated without making changes.</param>
    /// <param name="verbose">Show detailed output during the update process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(
        bool preview = false,
        bool nightly = false,
        bool dryRun = false,
        bool verbose = false)
    {
        AnsiConsole.Write(new FigletText("KBA Update").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Check and update KBA.Framework packages[/]" + Environment.NewLine);

        // Display configuration
        var configTable = new Table();
        configTable.AddColumn("Option");
        configTable.AddColumn("Value");
        configTable.AddRow("[cyan]Preview Channel[/]", preview ? "[green]Enabled[/]" : "[grey]Disabled[/]");
        configTable.AddRow("[cyan]Nightly Channel[/]", nightly ? "[green]Enabled[/]" : "[grey]Disabled[/]");
        configTable.AddRow("[cyan]Dry Run[/]", dryRun ? "[yellow]Yes (no changes)[/]" : "[green]No (will apply)[/]");
        configTable.AddRow("[cyan]Verbose[/]", verbose ? "[green]Enabled[/]" : "[grey]Disabled[/]");
        AnsiConsole.Write(configTable);
        AnsiConsole.WriteLine();

        // Find solution file
        var solutionPath = FindSolutionFile();
        if (solutionPath == null)
        {
            AnsiConsole.MarkupLine("[red]Error: No solution file (.sln) found in current directory.[/]");
            AnsiConsole.MarkupLine("[yellow]Please run this command from a solution directory.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[grey]Solution: {Path.GetFileName(solutionPath)}[/]");
        AnsiConsole.WriteLine();

        // Initialize services
        var console = AnsiConsole.Console;
        var scanner = new ProjectScannerService(console);
        var nugetService = new NuGetService(console);

        // Check for central package management
        var usesCentralManagement = scanner.UsesCentralPackageManagement(solutionPath);
        if (usesCentralManagement)
        {
            AnsiConsole.MarkupLine("[grey]Central Package Management: Enabled[/]");
        }

        AnsiConsole.WriteLine();

        // Scan for KBA.Framework packages
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Scanning project files for KBA.Framework packages...", async ctx =>
            {
                await ScanAndUpdatePackages(solutionPath, scanner, nugetService, preview, nightly, dryRun, verbose, usesCentralManagement);
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]✓ Update check completed![/]");

        if (dryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run completed. No changes were made.[/]");
            AnsiConsole.MarkupLine("[grey]Run without --dry-run to apply updates.[/]");
        }
    }

    /// <summary>
    /// Scans for packages and performs updates.
    /// </summary>
    private static async Task ScanAndUpdatePackages(
        string solutionPath,
        ProjectScannerService scanner,
        NuGetService nugetService,
        bool preview,
        bool nightly,
        bool dryRun,
        bool verbose,
        bool usesCentralManagement)
    {
        // Get all package references
        var allPackages = new Dictionary<string, PackageReferenceInfo>(StringComparer.OrdinalIgnoreCase);
        var projectFiles = scanner.GetProjectFilesFromSolution(solutionPath).ToList();

        if (projectFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Warning: No project files found in solution.[/]");
            return;
        }

        // Collect all package references
        foreach (var projectFile in projectFiles)
        {
            var references = scanner.GetPackageReferences(projectFile);
            var kbaPackages = ProjectScannerService.FilterKbaPackages(references);

            foreach (var pkg in kbaPackages)
            {
                if (!allPackages.ContainsKey(pkg.PackageName))
                {
                    allPackages[pkg.PackageName] = pkg;
                }
            }
        }

        if (allPackages.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No KBA.Framework packages found in the solution.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[grey]Found {allPackages.Count} KBA.Framework package(s)[/]");
        AnsiConsole.WriteLine();

        // Check for updates
        var updatesNeeded = new List<PackageUpdateInfo>();
        var upToDatePackages = new List<PackageUpdateInfo>();

        var progress = AnsiConsole.Progress()
            .AutoClear(false)
            .AutoRefresh(true)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            });

        await progress.StartAsync(async ctx =>
        {
            var checkTask = ctx.AddTask("[cyan]Checking for updates...[/]", maxValue: allPackages.Count);

            foreach (var kvp in allPackages)
            {
                var packageInfo = kvp.Value;
                checkTask.Description($"[cyan]Checking {packageInfo.PackageName}...[/]");

                var latestInfo = await nugetService.GetLatestVersionAsync(
                    packageInfo.PackageName,
                    preview,
                    nightly);

                if (latestInfo != null)
                {
                    latestInfo.CurrentVersion = packageInfo.Version;

                    if (latestInfo.HasUpdate)
                    {
                        updatesNeeded.Add(new PackageUpdateInfo
                        {
                            PackageName = packageInfo.PackageName,
                            CurrentVersion = packageInfo.Version!,
                            LatestVersion = latestInfo.LatestVersion,
                            IsPreview = latestInfo.IsPreview,
                            IsNightly = latestInfo.IsNightly,
                            DownloadUrl = latestInfo.DownloadUrl,
                            SourceFiles = new List<string> { packageInfo.SourceFile },
                            IsCentrallyManaged = packageInfo.IsCentrallyManaged
                        });
                    }
                    else
                    {
                        upToDatePackages.Add(new PackageUpdateInfo
                        {
                            PackageName = packageInfo.PackageName,
                            CurrentVersion = packageInfo.Version!,
                            LatestVersion = latestInfo.LatestVersion,
                            IsPreview = latestInfo.IsPreview,
                            IsNightly = latestInfo.IsNightly
                        });
                    }
                }
                else
                {
                    if (verbose)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Warning: Could not find package {packageInfo.PackageName} on NuGet.org[/]");
                    }
                }

                checkTask.Increment(1);
            }

            checkTask.Description("[green]Update check complete[/]");
        });

        // Display results
        DisplayUpdateResults(updatesNeeded, upToDatePackages, verbose);

        // Apply updates if not dry run
        if (updatesNeeded.Count > 0 && !dryRun)
        {
            await ApplyUpdates(updatesNeeded, scanner, solutionPath, usesCentralManagement, verbose);

            // Run dotnet restore
            await RunDotnetRestore(solutionPath);
        }
        else if (updatesNeeded.Count > 0 && dryRun)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]The following updates would be applied:[/]");
            foreach (var update in updatesNeeded)
            {
                var versionInfo = GetVersionBadge(update.IsPreview, update.IsNightly);
                AnsiConsole.MarkupLine($"  {versionInfo} [cyan]{update.PackageName}[/]: [grey]{update.CurrentVersion}[/] -> [green]{update.LatestVersion}[/]");
            }
        }
    }

    /// <summary>
    /// Displays the update results in a formatted table.
    /// </summary>
    private static void DisplayUpdateResults(List<PackageUpdateInfo> updatesNeeded, List<PackageUpdateInfo> upToDatePackages, bool verbose)
    {
        AnsiConsole.WriteLine();

        if (updatesNeeded.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold yellow]Updates Available:[/]");
            AnsiConsole.WriteLine();

            var table = new Table();
            table.AddColumn("Package");
            table.AddColumn("Current");
            table.AddColumn("Latest");
            table.AddColumn("Channel");
            table.AddColumn("Link");

            foreach (var update in updatesNeeded.OrderBy(u => u.PackageName))
            {
                var versionBadge = GetVersionBadge(update.IsPreview, update.IsNightly);
                var link = $"[blue][link={update.DownloadUrl}]NuGet[/][/]";

                table.AddRow(
                    $"[cyan]{update.PackageName}[/]",
                    $"[grey]{update.CurrentVersion}[/]",
                    $"[green]{update.LatestVersion}[/]",
                    versionBadge,
                    link
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.MarkupLine("[bold green]✓ All packages are up to date![/]");
        }

        if (verbose && upToDatePackages.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Up-to-date packages:[/]");
            foreach (var pkg in upToDatePackages.OrderBy(p => p.PackageName))
            {
                AnsiConsole.MarkupLine($"  [green]{pkg.PackageName}[/] ({pkg.CurrentVersion})");
            }
        }
    }

    /// <summary>
    /// Applies the pending updates to project files.
    /// </summary>
    private static async Task ApplyUpdates(
        List<PackageUpdateInfo> updates,
        ProjectScannerService scanner,
        string solutionPath,
        bool usesCentralManagement,
        bool verbose)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Applying updates...[/]");
        AnsiConsole.WriteLine();

        var updatedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var progress = AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
            });

        await progress.StartAsync(async ctx =>
        {
            var updateTask = ctx.AddTask("[cyan]Updating packages...[/]", maxValue: updates.Count);

            foreach (var update in updates)
            {
                updateTask.Description($"[cyan]Updating {update.PackageName}...[/]");

                bool success;

                // For centrally managed packages, update Directory.Packages.props
                if (usesCentralManagement && !updatedPackages.Contains(update.PackageName))
                {
                    success = scanner.UpdateCentralPackageVersion(solutionPath, update.PackageName, update.LatestVersion);
                    if (success)
                    {
                        updatedPackages.Add(update.PackageName);
                        if (verbose)
                        {
                            AnsiConsole.MarkupLine($"  [green]✓[/] Updated [cyan]{update.PackageName}[/] in Directory.Packages.props[/]");
                        }
                    }
                }

                // Update individual project files
                foreach (var sourceFile in update.SourceFiles.Distinct())
                {
                    success = scanner.UpdateProjectPackageVersion(sourceFile, update.PackageName, update.LatestVersion);
                    if (success && verbose)
                    {
                        AnsiConsole.MarkupLine($"  [green]✓[/] Updated [cyan]{update.PackageName}[/] in {Path.GetFileName(sourceFile)}[/]");
                    }
                }

                updateTask.Increment(1);
            }

            updateTask.Description("[green]Updates applied[/]");
        });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ Updated {updatedPackages.Count} package(s) successfully![/]");
    }

    /// <summary>
    /// Runs dotnet restore on the solution.
    /// </summary>
    private static async Task RunDotnetRestore(string solutionPath)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold blue]Running dotnet restore...[/]");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"restore \"{solutionPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(solutionPath) ?? "."
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    AnsiConsole.MarkupLine("[bold green]✓ dotnet restore completed successfully![/]");
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    AnsiConsole.MarkupLine($"[yellow]Warning: dotnet restore completed with warnings[/]");
                    if (!string.IsNullOrEmpty(error))
                    {
                        AnsiConsole.MarkupLine($"[grey]{error}[/]");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Failed to run dotnet restore: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[grey]Please run 'dotnet restore' manually.[/]");
        }
    }

    /// <summary>
    /// Finds the solution file in the current directory.
    /// </summary>
    private static string? FindSolutionFile()
    {
        var solutionFiles = Directory.GetFiles(".", "*.sln", SearchOption.TopDirectoryOnly);
        return solutionFiles.FirstOrDefault();
    }

    /// <summary>
    /// Gets a version badge string based on the version type.
    /// </summary>
    private static string GetVersionBadge(bool isPreview, bool isNightly)
    {
        if (isNightly)
        {
            return "[purple]Nightly[/]";
        }
        if (isPreview)
        {
            return "[yellow]Preview[/]";
        }
        return "[green]Stable[/]";
    }
}

/// <summary>
/// Represents information about a package update.
/// </summary>
public class PackageUpdateInfo
{
    /// <summary>
    /// Gets or sets the package name.
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current installed version.
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latest available version.
    /// </summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the latest version is a preview.
    /// </summary>
    public bool IsPreview { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the latest version is a nightly build.
    /// </summary>
    public bool IsNightly { get; set; }

    /// <summary>
    /// Gets or sets the NuGet download URL.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of source files containing this package reference.
    /// </summary>
    public List<string> SourceFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the package is centrally managed.
    /// </summary>
    public bool IsCentrallyManaged { get; set; }
}
