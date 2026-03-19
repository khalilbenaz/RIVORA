using System.Text.RegularExpressions;
using System.Xml.Linq;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides functionality to remove a module from a RIVORA Framework project.
/// Symmetric to <see cref="AddModuleCommand"/>.
/// </summary>
public static class RemoveModuleCommand
{
    // Known RVR modules and their typical Program.cs registration patterns
    private static readonly Dictionary<string, ModuleInfo> KnownModules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Caching"] = new("RVR.Framework.Caching", "AddRvrCaching", "UseRvrCaching", "Caching"),
        ["Email"] = new("RVR.Framework.Email", "AddRvrEmail", "UseRvrEmail", "Email"),
        ["Jobs"] = new("RVR.Framework.Jobs", "AddRvrJobs", "UseRvrJobs", "Jobs"),
        ["Events"] = new("RVR.Framework.Events", "AddRvrEvents", "UseRvrEvents", "Events"),
        ["Search"] = new("RVR.Framework.Search", "AddRvrSearch", "UseRvrSearch", "Search"),
        ["Notifications"] = new("RVR.Framework.Notifications", "AddRvrNotifications", "UseRvrNotifications", "Notifications"),
        ["Workflow"] = new("RVR.Framework.Workflow", "AddRvrWorkflow", "UseRvrWorkflow", "Workflow"),
        ["Export"] = new("RVR.Framework.Export", "AddRvrExport", "UseRvrExport", "Export"),
        ["Webhooks"] = new("RVR.Framework.Webhooks", "AddRvrWebhooks", "UseRvrWebhooks", "Webhooks"),
        ["AuditLogging"] = new("RVR.Framework.AuditLogging", "AddRvrAuditLogging", "UseRvrAuditLogging", "AuditLogging"),
        ["FeatureFlags"] = new("RVR.Framework.FeatureFlags", "AddRvrFeatureFlags", "UseRvrFeatureFlags", "FeatureFlags"),
        ["RateLimiting"] = new("RVR.Framework.RateLimiting", "AddRvrRateLimiting", "UseRvrRateLimiting", "RateLimiting"),
        ["AI"] = new("RVR.Framework.AI", "AddRvrAI", "UseRvrAI", "AI"),
        ["GraphQL"] = new("RVR.Framework.GraphQL", "AddRvrGraphQL", "UseRvrGraphQL", "GraphQL"),
        ["MultiTenancy"] = new("RVR.Framework.MultiTenancy", "AddRvrMultiTenancy", "UseRvrMultiTenancy", "MultiTenancy"),
        ["Security"] = new("RVR.Framework.Security", "AddRvrSecurity", "UseRvrSecurity", "Security"),
        ["Privacy"] = new("RVR.Framework.Privacy", "AddRvrPrivacy", "UseRvrPrivacy", "Privacy"),
    };

    /// <summary>
    /// Executes the remove-module command.
    /// </summary>
    public static async Task ExecuteAsync(string moduleName, bool dryRun, bool force)
    {
        AnsiConsole.Write(new FigletText("Remove Module").Color(Color.Red));
        AnsiConsole.MarkupLine("[grey]Remove a RIVORA module from the project[/]" + Environment.NewLine);

        // Normalize module name
        var normalizedName = NormalizeModuleName(moduleName);
        var moduleInfo = ResolveModuleInfo(normalizedName);

        AnsiConsole.MarkupLine($"[cyan]Module:[/] {normalizedName}");
        AnsiConsole.MarkupLine($"[cyan]Package:[/] {moduleInfo.PackageName}");
        AnsiConsole.MarkupLine($"[cyan]Dry Run:[/] {(dryRun ? "[yellow]Yes[/]" : "No")}");
        AnsiConsole.MarkupLine($"[cyan]Force:[/] {(force ? "[yellow]Yes[/]" : "No")}");
        AnsiConsole.WriteLine();

        var actions = new List<RemovalAction>();

        // 1. Find and collect all removal actions
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Scanning project for module references...", async ctx =>
            {
                actions.AddRange(await ScanForModuleReferences(normalizedName, moduleInfo));
            });

        if (actions.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No traces of module '{normalizedName}' found in the project.[/]");
            return;
        }

        // 2. Check dependencies
        var dependencyWarnings = CheckDependencies(normalizedName);
        if (dependencyWarnings.Count > 0 && !force)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Dependency warnings:[/]");
            foreach (var warning in dependencyWarnings)
            {
                AnsiConsole.MarkupLine($"  [yellow]⚠ {warning}[/]");
            }
            AnsiConsole.WriteLine();

            if (!dryRun)
            {
                var proceed = AnsiConsole.Confirm("[yellow]Continue with removal?[/]", false);
                if (!proceed)
                {
                    AnsiConsole.MarkupLine("[grey]Removal cancelled.[/]");
                    return;
                }
            }
        }

        // 3. Display planned actions
        AnsiConsole.WriteLine();
        var actionsTable = new Table();
        actionsTable.AddColumn("Action");
        actionsTable.AddColumn("File");
        actionsTable.AddColumn("Detail");
        foreach (var action in actions)
        {
            actionsTable.AddRow(
                $"[red]{action.Type}[/]",
                $"[cyan]{action.FilePath}[/]",
                $"[grey]{action.Description}[/]"
            );
        }
        AnsiConsole.Write(actionsTable);

        if (dryRun)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Dry run — no changes were made.[/]");
            AnsiConsole.MarkupLine("[grey]Run without --dry-run to apply the removal.[/]");
            return;
        }

        // 4. Apply removal actions
        AnsiConsole.WriteLine();
        var applied = 0;
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
                var task = ctx.AddTask("[red]Removing module references...[/]", maxValue: actions.Count);
                foreach (var action in actions)
                {
                    task.Description = $"[red]{action.Description}[/]";
                    ApplyAction(action);
                    applied++;
                    task.Increment(1);
                }
                task.Description = "[green]Removal complete[/]";
                await Task.CompletedTask;
            });

        // 5. Summary
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ Module '{normalizedName}' removed successfully![/]");
        AnsiConsole.MarkupLine($"[grey]{applied} change(s) applied.[/]");

        if (dependencyWarnings.Count > 0)
        {
            AnsiConsole.WriteLine();
            foreach (var warning in dependencyWarnings)
            {
                AnsiConsole.MarkupLine($"[yellow]⚠ {warning}[/]");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Next steps:[/]");
        AnsiConsole.MarkupLine("  1. Run [cyan]dotnet restore[/] to update dependencies");
        AnsiConsole.MarkupLine("  2. Run [cyan]dotnet build[/] to verify no broken references");
        AnsiConsole.MarkupLine("  3. Review and test your application");
    }

    private static string NormalizeModuleName(string input)
    {
        // Strip common prefixes
        var name = input
            .Replace("RVR.Framework.", "", StringComparison.OrdinalIgnoreCase)
            .Replace("RVR.", "", StringComparison.OrdinalIgnoreCase);
        return name;
    }

    private static ModuleInfo ResolveModuleInfo(string normalizedName)
    {
        if (KnownModules.TryGetValue(normalizedName, out var info))
            return info;

        // Fallback: construct from convention
        return new ModuleInfo(
            $"RVR.Framework.{normalizedName}",
            $"AddRvr{normalizedName}",
            $"UseRvr{normalizedName}",
            normalizedName
        );
    }

    private static async Task<List<RemovalAction>> ScanForModuleReferences(string moduleName, ModuleInfo info)
    {
        var actions = new List<RemovalAction>();
        var currentDir = Directory.GetCurrentDirectory();

        // 1. Scan .csproj files for PackageReference
        var csprojFiles = Directory.GetFiles(currentDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(csproj);
                if (content.Contains(info.PackageName, StringComparison.OrdinalIgnoreCase))
                {
                    actions.Add(new RemovalAction
                    {
                        Type = "Remove PackageReference",
                        FilePath = Path.GetRelativePath(currentDir, csproj),
                        Description = $"Remove <PackageReference> for {info.PackageName}",
                        FullPath = csproj,
                        ActionKind = ActionKind.RemovePackageReference,
                        SearchPattern = info.PackageName
                    });
                }
            }
            catch { /* skip unreadable files */ }
        }

        // 2. Scan Program.cs for Add*() and Use*() calls
        var programFiles = Directory.GetFiles(currentDir, "Program.cs", SearchOption.AllDirectories);
        foreach (var programFile in programFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(programFile);
                var relativePath = Path.GetRelativePath(currentDir, programFile);

                if (content.Contains(info.AddMethod, StringComparison.OrdinalIgnoreCase))
                {
                    actions.Add(new RemovalAction
                    {
                        Type = "Remove service registration",
                        FilePath = relativePath,
                        Description = $"Remove {info.AddMethod}() from Program.cs",
                        FullPath = programFile,
                        ActionKind = ActionKind.RemoveLineContaining,
                        SearchPattern = info.AddMethod
                    });
                }

                if (content.Contains(info.UseMethod, StringComparison.OrdinalIgnoreCase))
                {
                    actions.Add(new RemovalAction
                    {
                        Type = "Remove middleware",
                        FilePath = relativePath,
                        Description = $"Remove {info.UseMethod}() from Program.cs",
                        FullPath = programFile,
                        ActionKind = ActionKind.RemoveLineContaining,
                        SearchPattern = info.UseMethod
                    });
                }
            }
            catch { }
        }

        // 3. Scan appsettings*.json for module section
        var appSettingsFiles = Directory.GetFiles(currentDir, "appsettings*.json", SearchOption.AllDirectories);
        foreach (var settingsFile in appSettingsFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(settingsFile);
                if (content.Contains($"\"{info.ConfigSection}\"", StringComparison.OrdinalIgnoreCase))
                {
                    actions.Add(new RemovalAction
                    {
                        Type = "Remove config section",
                        FilePath = Path.GetRelativePath(currentDir, settingsFile),
                        Description = $"Remove \"{info.ConfigSection}\" section from appsettings",
                        FullPath = settingsFile,
                        ActionKind = ActionKind.RemoveJsonSection,
                        SearchPattern = info.ConfigSection
                    });
                }
            }
            catch { }
        }

        // 4. Check for dedicated config files
        var configPatterns = new[] { $"{moduleName.ToLower()}.json", $"{moduleName.ToLower()}.config" };
        foreach (var pattern in configPatterns)
        {
            var configFiles = Directory.GetFiles(currentDir, pattern, SearchOption.AllDirectories);
            foreach (var configFile in configFiles)
            {
                actions.Add(new RemovalAction
                {
                    Type = "Delete config file",
                    FilePath = Path.GetRelativePath(currentDir, configFile),
                    Description = $"Delete dedicated config file",
                    FullPath = configFile,
                    ActionKind = ActionKind.DeleteFile,
                    SearchPattern = ""
                });
            }
        }

        return actions;
    }

    private static List<string> CheckDependencies(string moduleName)
    {
        var warnings = new List<string>();

        // Known dependency relationships
        var dependencyMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Caching"] = new[] { "Notifications", "Jobs", "Search" },
            ["Events"] = new[] { "Notifications", "Workflow", "Webhooks" },
            ["Security"] = new[] { "MultiTenancy", "Privacy", "AuditLogging" },
            ["MultiTenancy"] = new[] { "SaaS", "Billing" },
        };

        // Check if any other module depends on this one
        foreach (var (module, deps) in dependencyMap)
        {
            if (deps.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
            {
                // Check if that module is actually used in the project
                var csprojFiles = Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories);
                foreach (var csproj in csprojFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(csproj);
                        if (content.Contains($"RVR.Framework.{module}", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Module {module} depends on {moduleName} — verify your configuration");
                            break;
                        }
                    }
                    catch { }
                }
            }
        }

        return warnings;
    }

    private static void ApplyAction(RemovalAction action)
    {
        switch (action.ActionKind)
        {
            case ActionKind.RemovePackageReference:
                RemovePackageReference(action.FullPath, action.SearchPattern);
                break;
            case ActionKind.RemoveLineContaining:
                RemoveLineContaining(action.FullPath, action.SearchPattern);
                break;
            case ActionKind.RemoveJsonSection:
                RemoveJsonSection(action.FullPath, action.SearchPattern);
                break;
            case ActionKind.DeleteFile:
                if (File.Exists(action.FullPath))
                    File.Delete(action.FullPath);
                break;
        }
    }

    private static void RemovePackageReference(string csprojPath, string packageName)
    {
        try
        {
            var doc = XDocument.Load(csprojPath);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
            var packageRefs = doc.Descendants(ns + "PackageReference")
                .Where(e => string.Equals(
                    e.Attribute("Include")?.Value,
                    packageName,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var pr in packageRefs)
                pr.Remove();

            // Clean up empty ItemGroups
            var emptyGroups = doc.Descendants(ns + "ItemGroup")
                .Where(g => !g.HasElements)
                .ToList();
            foreach (var eg in emptyGroups)
                eg.Remove();

            doc.Save(csprojPath);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Could not update {csprojPath}: {ex.Message}[/]");
        }
    }

    private static void RemoveLineContaining(string filePath, string pattern)
    {
        try
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var newLines = lines
                .Where(line => !line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Remove consecutive blank lines left behind
            var cleaned = new List<string>();
            var prevBlank = false;
            foreach (var line in newLines)
            {
                var isBlank = string.IsNullOrWhiteSpace(line);
                if (isBlank && prevBlank) continue;
                cleaned.Add(line);
                prevBlank = isBlank;
            }

            File.WriteAllLines(filePath, cleaned);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Could not update {filePath}: {ex.Message}[/]");
        }
    }

    private static void RemoveJsonSection(string filePath, string sectionName)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            // Remove a top-level JSON section like "SectionName": { ... }
            var pattern = $@",?\s*""{Regex.Escape(sectionName)}""\s*:\s*\{{[^{{}}]*\}}";
            var result = Regex.Replace(content, pattern, "", RegexOptions.Singleline);

            // Clean up leading comma after opening brace
            result = Regex.Replace(result, @"\{\s*,", "{");

            File.WriteAllText(filePath, result);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Could not update {filePath}: {ex.Message}[/]");
        }
    }

    private record ModuleInfo(string PackageName, string AddMethod, string UseMethod, string ConfigSection);

    private enum ActionKind
    {
        RemovePackageReference,
        RemoveLineContaining,
        RemoveJsonSection,
        DeleteFile
    }

    private class RemovalAction
    {
        public string Type { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Description { get; set; } = "";
        public string FullPath { get; set; } = "";
        public ActionKind ActionKind { get; set; }
        public string SearchPattern { get; set; } = "";
    }
}
