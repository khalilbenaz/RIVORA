using System.Diagnostics;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides unified publish functionality for RIVORA projects (Docker, NuGet, Azure, self-contained).
/// </summary>
public static class PublishCommand
{
    /// <summary>
    /// Executes the publish command.
    /// </summary>
    public static async Task ExecuteAsync(
        string target = "auto",
        bool skipTests = false,
        bool dryRun = false,
        string? registry = null,
        string? tag = null)
    {
        AnsiConsole.Write(new FigletText("RVR Publish").Color(Color.Purple));
        AnsiConsole.MarkupLine("[grey]Unified publishing for RIVORA projects[/]" + Environment.NewLine);

        var currentDir = Directory.GetCurrentDirectory();
        var solutionFile = Directory.GetFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (solutionFile == null)
        {
            AnsiConsole.MarkupLine("[red]Error: No solution file (.sln) found in current directory.[/]");
            return;
        }

        // Auto-detect target if not specified
        if (target == "auto")
        {
            target = AutoDetectTarget(currentDir);
            AnsiConsole.MarkupLine($"[grey]Auto-detected target: [cyan]{target}[/][/]");
        }

        // Detect project version
        var version = tag ?? DetectProjectVersion(currentDir);

        // Display configuration
        var configTable = new Table();
        configTable.AddColumn("Option");
        configTable.AddColumn("Value");
        configTable.AddRow("[cyan]Target[/]", $"[green]{target}[/]");
        configTable.AddRow("[cyan]Version[/]", $"[green]{version}[/]");
        configTable.AddRow("[cyan]Skip Tests[/]", skipTests ? "[yellow]Yes[/]" : "[grey]No[/]");
        configTable.AddRow("[cyan]Dry Run[/]", dryRun ? "[yellow]Yes[/]" : "[grey]No[/]");
        configTable.AddRow("[cyan]Registry[/]", registry ?? "[grey]default[/]");
        AnsiConsole.Write(configTable);
        AnsiConsole.WriteLine();

        var steps = BuildPublishSteps(target, currentDir, solutionFile, version, skipTests, registry);

        if (dryRun)
        {
            AnsiConsole.MarkupLine("[bold yellow]Dry run — commands that would be executed:[/]");
            AnsiConsole.WriteLine();
            foreach (var step in steps)
            {
                AnsiConsole.MarkupLine($"  [grey]$[/] [cyan]{step.Command}[/]");
                AnsiConsole.MarkupLine($"    [grey]{step.Description}[/]");
            }
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]No changes were made.[/]");
            return;
        }

        // Execute publish pipeline
        var success = true;
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
                var task = ctx.AddTask("[purple]Publishing...[/]", maxValue: steps.Count);

                foreach (var step in steps)
                {
                    task.Description = $"[purple]{step.Description}[/]";
                    var result = await RunCommandAsync(step.Command, currentDir);

                    if (!result.Success)
                    {
                        AnsiConsole.MarkupLine($"[red]✗ {step.Description} failed[/]");
                        if (!string.IsNullOrEmpty(result.Error))
                            AnsiConsole.MarkupLine($"[red]{result.Error}[/]");
                        success = false;
                        break;
                    }

                    task.Increment(1);
                }

                task.Description = success ? "[green]Publish complete[/]" : "[red]Publish failed[/]";
            });

        AnsiConsole.WriteLine();
        if (success)
        {
            AnsiConsole.MarkupLine($"[bold green]✓ Published successfully![/]");
            DisplayPostPublishInfo(target, version, registry);
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]✗ Publish failed. Check the errors above.[/]");
        }
    }

    private static string AutoDetectTarget(string dir)
    {
        if (File.Exists(Path.Combine(dir, "Dockerfile")))
            return "docker";

        // Check for packable projects
        var csprojFiles = Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojFiles)
        {
            try
            {
                var content = File.ReadAllText(csproj);
                if (content.Contains("<IsPackable>true</IsPackable>", StringComparison.OrdinalIgnoreCase))
                    return "nuget";
            }
            catch { }
        }

        return "self-contained";
    }

    private static string DetectProjectVersion(string dir)
    {
        // Check Directory.Build.props first
        var buildProps = Path.Combine(dir, "Directory.Build.props");
        if (File.Exists(buildProps))
        {
            var content = File.ReadAllText(buildProps);
            var match = System.Text.RegularExpressions.Regex.Match(content, @"<Version>([\w\.\-]+)</Version>");
            if (match.Success) return match.Groups[1].Value;
        }

        // Check first .csproj
        var csproj = Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
        if (csproj != null)
        {
            var content = File.ReadAllText(csproj);
            var match = System.Text.RegularExpressions.Regex.Match(content, @"<Version>([\w\.\-]+)</Version>");
            if (match.Success) return match.Groups[1].Value;
        }

        return "1.0.0";
    }

    private static List<PublishStep> BuildPublishSteps(
        string target, string dir, string solutionFile, string version,
        bool skipTests, string? registry)
    {
        var steps = new List<PublishStep>();
        var sln = Path.GetFileName(solutionFile);

        // Step 1: Build
        steps.Add(new PublishStep
        {
            Description = "Building solution...",
            Command = $"dotnet build \"{sln}\" -c Release --nologo"
        });

        // Step 2: Tests (unless skipped)
        if (!skipTests)
        {
            steps.Add(new PublishStep
            {
                Description = "Running tests...",
                Command = $"dotnet test \"{sln}\" -c Release --no-build --nologo"
            });
        }

        // Step 3+: Target-specific
        switch (target.ToLowerInvariant())
        {
            case "docker":
                BuildDockerSteps(steps, dir, version, registry);
                break;
            case "nuget":
                BuildNuGetSteps(steps, dir, version, registry);
                break;
            case "self-contained":
                BuildSelfContainedSteps(steps, dir, version);
                break;
            case "azure":
                BuildAzureSteps(steps, dir, version);
                break;
        }

        return steps;
    }

    private static void BuildDockerSteps(List<PublishStep> steps, string dir, string version, string? registry)
    {
        var imageName = Path.GetFileName(dir).ToLowerInvariant().Replace(".", "-");
        var fullTag = registry != null
            ? $"{registry}/{imageName}:{version}"
            : $"{imageName}:{version}";

        // Generate Dockerfile if missing
        if (!File.Exists(Path.Combine(dir, "Dockerfile")))
        {
            steps.Add(new PublishStep
            {
                Description = "Generating optimized Dockerfile...",
                Command = "rvr generate docker"
            });
        }

        steps.Add(new PublishStep
        {
            Description = "Building Docker image...",
            Command = $"docker build -t {fullTag} -t {imageName}:latest ."
        });

        if (registry != null)
        {
            steps.Add(new PublishStep
            {
                Description = $"Pushing to {registry}...",
                Command = $"docker push {fullTag}"
            });

            steps.Add(new PublishStep
            {
                Description = "Pushing latest tag...",
                Command = $"docker push {imageName}:latest"
            });
        }
    }

    private static void BuildNuGetSteps(List<PublishStep> steps, string dir, string version, string? registry)
    {
        var outputDir = Path.Combine(dir, "artifacts", "packages");

        steps.Add(new PublishStep
        {
            Description = "Packing NuGet packages...",
            Command = $"dotnet pack -c Release -o \"{outputDir}\" /p:Version={version} --no-build --nologo"
        });

        var source = registry ?? "https://api.nuget.org/v3/index.json";
        steps.Add(new PublishStep
        {
            Description = $"Pushing packages to {(registry ?? "NuGet.org")}...",
            Command = $"dotnet nuget push \"{outputDir}/*.nupkg\" --source \"{source}\" --skip-duplicate"
        });
    }

    private static void BuildSelfContainedSteps(List<PublishStep> steps, string dir, string version)
    {
        var rids = new[] { "win-x64", "linux-x64", "osx-x64" };
        var apiProject = FindApiProject(dir);

        if (apiProject == null)
        {
            steps.Add(new PublishStep
            {
                Description = "Publishing self-contained...",
                Command = $"dotnet publish -c Release --self-contained -o artifacts/publish"
            });
            return;
        }

        foreach (var rid in rids)
        {
            var outputDir = Path.Combine("artifacts", "publish", rid);
            steps.Add(new PublishStep
            {
                Description = $"Publishing for {rid}...",
                Command = $"dotnet publish \"{apiProject}\" -c Release -r {rid} --self-contained -o \"{outputDir}\" /p:Version={version}"
            });
        }
    }

    private static void BuildAzureSteps(List<PublishStep> steps, string dir, string version)
    {
        var apiProject = FindApiProject(dir);
        var projectArg = apiProject != null ? $" \"{apiProject}\"" : "";

        steps.Add(new PublishStep
        {
            Description = "Publishing for Azure...",
            Command = $"dotnet publish{projectArg} -c Release -o artifacts/azure-publish"
        });

        steps.Add(new PublishStep
        {
            Description = "Deploying to Azure (requires az login)...",
            Command = "az webapp deploy --src-path artifacts/azure-publish"
        });
    }

    private static string? FindApiProject(string dir)
    {
        return Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains(".Api", StringComparison.OrdinalIgnoreCase) ||
                                 f.Contains(".WebApi", StringComparison.OrdinalIgnoreCase));
    }

    private static void DisplayPostPublishInfo(string target, string version, string? registry)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Post-publish info:[/]");

        switch (target.ToLowerInvariant())
        {
            case "docker":
                var imageName = Path.GetFileName(Directory.GetCurrentDirectory()).ToLowerInvariant().Replace(".", "-");
                AnsiConsole.MarkupLine($"  Image: [cyan]{imageName}:{version}[/]");
                AnsiConsole.MarkupLine($"  Run: [cyan]docker run -p 8080:8080 {imageName}:{version}[/]");
                break;
            case "nuget":
                AnsiConsole.MarkupLine($"  Packages published to [cyan]{registry ?? "NuGet.org"}[/]");
                AnsiConsole.MarkupLine($"  Version: [cyan]{version}[/]");
                break;
            case "self-contained":
                AnsiConsole.MarkupLine("  Binaries available in [cyan]artifacts/publish/[/]");
                break;
            case "azure":
                AnsiConsole.MarkupLine("  Check Azure portal for deployment status");
                break;
        }
    }

    private static async Task<CommandResult> RunCommandAsync(string command, string workingDir)
    {
        try
        {
            var parts = command.Split(' ', 2);
            var psi = new ProcessStartInfo
            {
                FileName = parts[0],
                Arguments = parts.Length > 1 ? parts[1] : "",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDir
            };

            using var process = Process.Start(psi);
            if (process == null)
                return new CommandResult { Success = false, Error = "Failed to start process" };

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new CommandResult
            {
                Success = process.ExitCode == 0,
                Output = output,
                Error = error
            };
        }
        catch (Exception ex)
        {
            return new CommandResult { Success = false, Error = ex.Message };
        }
    }

    private class PublishStep
    {
        public string Description { get; set; } = "";
        public string Command { get; set; } = "";
    }

    private class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
    }
}
