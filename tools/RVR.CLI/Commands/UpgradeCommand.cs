using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RVR.CLI.Services;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides migration assistance between major RIVORA Framework versions.
/// Distinct from <see cref="UpdateCommand"/> which updates the CLI itself.
/// </summary>
public static class UpgradeCommand
{
    /// <summary>
    /// Executes the upgrade command.
    /// </summary>
    public static async Task ExecuteAsync(
        string? targetVersion = null,
        bool dryRun = false,
        bool list = false)
    {
        AnsiConsole.Write(new FigletText("RVR Upgrade").Color(Color.Orange1));
        AnsiConsole.MarkupLine("[grey]Migration assistant for RIVORA Framework[/]" + Environment.NewLine);

        var currentDir = Directory.GetCurrentDirectory();
        var solutionFile = Directory.GetFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (solutionFile == null)
        {
            AnsiConsole.MarkupLine("[red]Error: No solution file (.sln) found in current directory.[/]");
            return;
        }

        // Detect current version
        var currentVersion = await DetectCurrentVersion(currentDir);
        AnsiConsole.MarkupLine($"[cyan]Current version:[/] [green]{currentVersion}[/]");

        // Load available migrations
        var migrations = LoadMigrations();

        if (list)
        {
            DisplayAvailableMigrations(migrations, currentVersion);
            return;
        }

        // Determine target
        var target = targetVersion ?? migrations.LastOrDefault()?.TargetVersion ?? currentVersion;
        AnsiConsole.MarkupLine($"[cyan]Target version:[/]  [green]{target}[/]");
        AnsiConsole.MarkupLine($"[cyan]Dry run:[/]         {(dryRun ? "[yellow]Yes[/]" : "No")}");
        AnsiConsole.WriteLine();

        // Find applicable migrations
        var applicable = migrations
            .Where(m => CompareVersions(m.FromVersion, currentVersion) <= 0 &&
                        CompareVersions(m.TargetVersion, target) <= 0 &&
                        CompareVersions(m.TargetVersion, currentVersion) > 0)
            .OrderBy(m => m.TargetVersion)
            .ToList();

        if (applicable.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Already up to date! No migrations needed.[/]");
            return;
        }

        // Display migration plan
        AnsiConsole.MarkupLine($"[bold]Migration plan ({applicable.Count} step(s)):[/]");
        AnsiConsole.WriteLine();

        var planTable = new Table();
        planTable.AddColumn("#");
        planTable.AddColumn("Migration");
        planTable.AddColumn("Breaking Changes");
        planTable.AddColumn("Auto-fixable");

        for (var i = 0; i < applicable.Count; i++)
        {
            var m = applicable[i];
            planTable.AddRow(
                $"[grey]{i + 1}[/]",
                $"[cyan]{m.FromVersion} → {m.TargetVersion}[/]",
                $"[yellow]{m.BreakingChanges.Count}[/]",
                $"[green]{m.Transformations.Count}[/]"
            );
        }
        AnsiConsole.Write(planTable);
        AnsiConsole.WriteLine();

        // Collect all changes to apply
        var allChanges = new List<UpgradeAction>();
        foreach (var migration in applicable)
        {
            allChanges.AddRange(await AnalyzeMigration(migration, currentDir));
        }

        if (allChanges.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No changes needed for your project.[/]");
            return;
        }

        // Display changes
        var changesTable = new Table();
        changesTable.AddColumn("Type");
        changesTable.AddColumn("File");
        changesTable.AddColumn("Change");
        changesTable.AddColumn("Auto");

        foreach (var change in allChanges)
        {
            changesTable.AddRow(
                change.IsAutomatic ? "[green]Auto[/]" : "[yellow]Manual[/]",
                $"[cyan]{change.FilePath}[/]",
                $"[grey]{change.Description}[/]",
                change.IsAutomatic ? "[green]✓[/]" : "[yellow]—[/]"
            );
        }
        AnsiConsole.Write(changesTable);

        if (dryRun)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Dry run — no changes were made.[/]");

            var manualChanges = allChanges.Where(c => !c.IsAutomatic).ToList();
            if (manualChanges.Count > 0)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[bold yellow]Manual changes required:[/]");
                foreach (var mc in manualChanges)
                {
                    AnsiConsole.MarkupLine($"  [yellow]⚠[/] {mc.Description}");
                    if (!string.IsNullOrEmpty(mc.Documentation))
                        AnsiConsole.MarkupLine($"    [grey]See: {mc.Documentation}[/]");
                }
            }
            return;
        }

        // Apply automatic changes
        var autoChanges = allChanges.Where(c => c.IsAutomatic).ToList();
        if (autoChanges.Count > 0)
        {
            AnsiConsole.WriteLine();
            await AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn(),
                })
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[orange1]Applying migrations...[/]", maxValue: autoChanges.Count);

                    foreach (var change in autoChanges)
                    {
                        task.Description = $"[orange1]{change.Description}[/]";
                        await ApplyChange(change);
                        task.Increment(1);
                    }

                    task.Description = "[green]Migrations applied[/]";
                });
        }

        // Run dotnet restore
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Running dotnet restore...[/]");
        await RunProcessAsync("dotnet", $"restore \"{solutionFile}\"", currentDir);

        // Final report
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ Upgrade to {target} completed![/]");

        var manualRemaining = allChanges.Where(c => !c.IsAutomatic).ToList();
        if (manualRemaining.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Manual steps remaining:[/]");
            for (var i = 0; i < manualRemaining.Count; i++)
            {
                var mc = manualRemaining[i];
                AnsiConsole.MarkupLine($"  {i + 1}. {mc.Description}");
                if (!string.IsNullOrEmpty(mc.Documentation))
                    AnsiConsole.MarkupLine($"     [grey]See: {mc.Documentation}[/]");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Next steps:[/]");
        AnsiConsole.MarkupLine("  1. Run [cyan]dotnet build[/] to verify compilation");
        AnsiConsole.MarkupLine("  2. Run [cyan]dotnet test[/] to verify tests pass");
        AnsiConsole.MarkupLine("  3. Review and test your application");
    }

    private static async Task<string> DetectCurrentVersion(string dir)
    {
        // Check Directory.Build.props
        var buildProps = Path.Combine(dir, "Directory.Build.props");
        if (File.Exists(buildProps))
        {
            var content = await File.ReadAllTextAsync(buildProps);
            var match = Regex.Match(content, @"<Version>([\d\.]+)");
            if (match.Success) return match.Groups[1].Value;
        }

        // Check csproj files for RVR.Framework package versions
        var csprojFiles = Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(csproj);
                var match = Regex.Match(content, @"RVR\.Framework\.\w+""\s+Version=""([\d\.]+)");
                if (match.Success) return match.Groups[1].Value;
            }
            catch { }
        }

        // Check Directory.Packages.props
        var packagesProps = Path.Combine(dir, "Directory.Packages.props");
        if (File.Exists(packagesProps))
        {
            var content = await File.ReadAllTextAsync(packagesProps);
            var match = Regex.Match(content, @"RVR\.Framework\.\w+""\s+Version=""([\d\.]+)");
            if (match.Success) return match.Groups[1].Value;
        }

        return "0.0.0";
    }

    private static List<MigrationDefinition> LoadMigrations()
    {
        // Built-in migration definitions
        return new List<MigrationDefinition>
        {
            new()
            {
                FromVersion = "3.0.0",
                TargetVersion = "3.1.0",
                BreakingChanges = new List<string>
                {
                    "IRvrRepository renamed to IRepository",
                    "AddRvrFramework() split into AddRvrCore() + AddRvrInfrastructure()"
                },
                Transformations = new List<TransformRule>
                {
                    new() { Pattern = "IRvrRepository", Replacement = "IRepository", FileGlob = "*.cs" },
                    new() { Pattern = "AddRvrFramework()", Replacement = "AddRvrCore().AddRvrInfrastructure()", FileGlob = "Program.cs" },
                }
            },
            new()
            {
                FromVersion = "3.1.0",
                TargetVersion = "3.2.0",
                BreakingChanges = new List<string>
                {
                    "MultiTenancyMode enum values renamed (Row -> RowLevel, Schema -> SchemaLevel)",
                    "IEventBus moved from Core to Events module"
                },
                Transformations = new List<TransformRule>
                {
                    new() { Pattern = "MultiTenancyMode.Row", Replacement = "MultiTenancyMode.RowLevel", FileGlob = "*.cs" },
                    new() { Pattern = "MultiTenancyMode.Schema", Replacement = "MultiTenancyMode.SchemaLevel", FileGlob = "*.cs" },
                    new() { Pattern = "using RVR.Framework.Core.Events", Replacement = "using RVR.Framework.Events", FileGlob = "*.cs" },
                }
            },
            new()
            {
                FromVersion = "3.2.0",
                TargetVersion = "3.3.0",
                BreakingChanges = new List<string>
                {
                    "Minimal API registration pattern changed",
                    "Privacy module extracted from Security"
                },
                Transformations = new List<TransformRule>
                {
                    new() { Pattern = "MapRvrEndpoints()", Replacement = "MapRvrApi()", FileGlob = "Program.cs" },
                    new() { Pattern = "AddRvrSecurity().WithPrivacy()", Replacement = "AddRvrSecurity().AddRvrPrivacy()", FileGlob = "Program.cs" },
                }
            },
            new()
            {
                FromVersion = "3.3.0",
                TargetVersion = "4.0.0",
                BreakingChanges = new List<string>
                {
                    "Target framework upgraded to .NET 9.0",
                    "Entity base class simplified (IAuditable merged into Entity)",
                    "Configuration section 'Kba' renamed to 'Rvr'",
                    "All AddKba*() methods renamed to AddRvr*()",
                    "Package prefix changed from KBA.Framework to RVR.Framework",
                },
                Transformations = new List<TransformRule>
                {
                    new() { Pattern = "net8.0", Replacement = "net9.0", FileGlob = "*.csproj" },
                    new() { Pattern = "AddKba", Replacement = "AddRvr", FileGlob = "*.cs" },
                    new() { Pattern = "UseKba", Replacement = "UseRvr", FileGlob = "*.cs" },
                    new() { Pattern = "KBA.Framework", Replacement = "RVR.Framework", FileGlob = "*.cs" },
                    new() { Pattern = "KBA.Framework", Replacement = "RVR.Framework", FileGlob = "*.csproj" },
                    new() { Pattern = "\"Kba\"", Replacement = "\"Rvr\"", FileGlob = "appsettings*.json" },
                },
                ManualSteps = new List<ManualStep>
                {
                    new() { Description = "Remove IAuditable implementation from entities — audit fields are now in Entity base class", Documentation = "https://github.com/khalilbenaz/RIVORA/wiki/v4-migration" },
                    new() { Description = "Review and update any custom middleware that references the old pipeline", Documentation = "https://github.com/khalilbenaz/RIVORA/wiki/v4-middleware" },
                }
            }
        };
    }

    private static void DisplayAvailableMigrations(List<MigrationDefinition> migrations, string currentVersion)
    {
        AnsiConsole.MarkupLine("[bold]Available migrations:[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("From");
        table.AddColumn("To");
        table.AddColumn("Breaking Changes");
        table.AddColumn("Auto Transforms");
        table.AddColumn("Status");

        foreach (var m in migrations)
        {
            var status = CompareVersions(m.TargetVersion, currentVersion) <= 0
                ? "[green]Applied[/]"
                : "[yellow]Pending[/]";

            table.AddRow(
                $"[cyan]{m.FromVersion}[/]",
                $"[cyan]{m.TargetVersion}[/]",
                $"[yellow]{m.BreakingChanges.Count}[/]",
                $"[green]{m.Transformations.Count}[/]",
                status
            );
        }

        AnsiConsole.Write(table);

        // Show details for pending migrations
        var pending = migrations.Where(m => CompareVersions(m.TargetVersion, currentVersion) > 0).ToList();
        if (pending.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Pending breaking changes:[/]");
            foreach (var m in pending)
            {
                AnsiConsole.MarkupLine($"\n  [bold cyan]{m.FromVersion} → {m.TargetVersion}[/]");
                foreach (var bc in m.BreakingChanges)
                    AnsiConsole.MarkupLine($"    [yellow]⚠[/] {bc}");
            }
        }
    }

    private static async Task<List<UpgradeAction>> AnalyzeMigration(MigrationDefinition migration, string dir)
    {
        var actions = new List<UpgradeAction>();

        // Check transformations
        foreach (var transform in migration.Transformations)
        {
            var files = Directory.GetFiles(dir, transform.FileGlob, SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                            !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

            foreach (var file in files)
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    if (content.Contains(transform.Pattern))
                    {
                        actions.Add(new UpgradeAction
                        {
                            FilePath = Path.GetRelativePath(dir, file),
                            FullPath = file,
                            Description = $"Replace '{transform.Pattern}' → '{transform.Replacement}'",
                            IsAutomatic = true,
                            OldValue = transform.Pattern,
                            NewValue = transform.Replacement
                        });
                    }
                }
                catch { }
            }
        }

        // Update package versions
        var csprojFiles = Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

        foreach (var csproj in csprojFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(csproj);
                if (content.Contains("RVR.Framework") &&
                    Regex.IsMatch(content, $@"Version=""{Regex.Escape(migration.FromVersion)}"))
                {
                    actions.Add(new UpgradeAction
                    {
                        FilePath = Path.GetRelativePath(dir, csproj),
                        FullPath = csproj,
                        Description = $"Update package versions {migration.FromVersion} → {migration.TargetVersion}",
                        IsAutomatic = true,
                        OldValue = $"Version=\"{migration.FromVersion}",
                        NewValue = $"Version=\"{migration.TargetVersion}"
                    });
                }
            }
            catch { }
        }

        // Add manual steps
        if (migration.ManualSteps != null)
        {
            foreach (var step in migration.ManualSteps)
            {
                actions.Add(new UpgradeAction
                {
                    FilePath = "—",
                    FullPath = "",
                    Description = step.Description,
                    IsAutomatic = false,
                    Documentation = step.Documentation
                });
            }
        }

        return actions;
    }

    private static async Task ApplyChange(UpgradeAction change)
    {
        if (string.IsNullOrEmpty(change.FullPath) || !File.Exists(change.FullPath))
            return;

        try
        {
            var content = await File.ReadAllTextAsync(change.FullPath);
            content = content.Replace(change.OldValue, change.NewValue);
            await File.WriteAllTextAsync(change.FullPath, content);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Could not update {change.FilePath}: {ex.Message}[/]");
        }
    }

    private static int CompareVersions(string v1, string v2)
    {
        var parts1 = v1.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
        var parts2 = v2.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();

        for (var i = 0; i < Math.Max(parts1.Length, parts2.Length); i++)
        {
            var a = i < parts1.Length ? parts1[i] : 0;
            var b = i < parts2.Length ? parts2[i] : 0;
            if (a != b) return a.CompareTo(b);
        }
        return 0;
    }

    private static async Task RunProcessAsync(string fileName, string arguments, string workingDir)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDir
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                    AnsiConsole.MarkupLine("[green]✓[/] Done");
                else
                    AnsiConsole.MarkupLine("[yellow]⚠ Completed with warnings[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: {ex.Message}[/]");
        }
    }

    private class MigrationDefinition
    {
        public string FromVersion { get; set; } = "";
        public string TargetVersion { get; set; } = "";
        public List<string> BreakingChanges { get; set; } = new();
        public List<TransformRule> Transformations { get; set; } = new();
        public List<ManualStep>? ManualSteps { get; set; }
    }

    private class TransformRule
    {
        public string Pattern { get; set; } = "";
        public string Replacement { get; set; } = "";
        public string FileGlob { get; set; } = "*.*";
    }

    private class ManualStep
    {
        public string Description { get; set; } = "";
        public string Documentation { get; set; } = "";
    }

    private class UpgradeAction
    {
        public string FilePath { get; set; } = "";
        public string FullPath { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsAutomatic { get; set; }
        public string OldValue { get; set; } = "";
        public string NewValue { get; set; } = "";
        public string? Documentation { get; set; }
    }
}
