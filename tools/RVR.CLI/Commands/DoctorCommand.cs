using Spectre.Console;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides comprehensive project diagnostics and health checks.
/// </summary>
public static class DoctorCommand
{
    /// <summary>
    /// Executes the doctor command to diagnose project issues.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync()
    {
        AnsiConsole.Write(new FigletText("RVR Doctor").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Comprehensive project diagnostics[/]" + Environment.NewLine);

        var results = new List<(string Category, string Check, bool Passed, string Details)>();

        // Environment checks
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Checking environment...", async ctx =>
            {
                results.Add(CheckDotNetSdk());
                results.Add(CheckNodeJs());
                results.Add(CheckDocker());
                results.Add(CheckGit());
            });

        // Project structure checks
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Analyzing project structure...", async ctx =>
            {
                results.AddRange(CheckProjectStructure());
            });

        // Dependencies checks
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Checking dependencies...", async ctx =>
            {
                results.AddRange(CheckDependencies());
            });

        // Configuration checks
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Validating configuration...", async ctx =>
            {
                results.AddRange(CheckConfiguration());
            });

        // Display results
        AnsiConsole.WriteLine();
        DisplayResults(results);

        // Summary
        var failed = results.Count(r => !r.Passed);
        if (failed == 0)
        {
            AnsiConsole.MarkupLine("[bold green]✓ All checks passed! Your project is healthy.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold yellow]⚠ {failed} check(s) failed. Review the issues above.[/]");
        }

        await Task.CompletedTask;
    }

    private static (string Category, string Check, bool Passed, string Details) CheckDotNetSdk()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            var version = process?.StandardOutput.ReadToEnd().Trim() ?? "Unknown";
            var isCorrectVersion = version.StartsWith("8.");
            return ("Environment", ".NET SDK", isCorrectVersion, version);
        }
        catch
        {
            return ("Environment", ".NET SDK", false, "Not installed");
        }
    }

    private static (string Category, string Check, bool Passed, string Details) CheckNodeJs()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            var version = process?.StandardOutput.ReadToEnd().Trim() ?? "Unknown";
            return ("Environment", "Node.js", true, version);
        }
        catch
        {
            return ("Environment", "Node.js", false, "Not installed");
        }
    }

    private static (string Category, string Check, bool Passed, string Details) CheckDocker()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            var version = process?.StandardOutput.ReadToEnd().Trim() ?? "Unknown";
            return ("Environment", "Docker", true, version);
        }
        catch
        {
            return ("Environment", "Docker", false, "Not installed");
        }
    }

    private static (string Category, string Check, bool Passed, string Details) CheckGit()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
            var version = process?.StandardOutput.ReadToEnd().Trim() ?? "Unknown";
            return ("Environment", "Git", true, version);
        }
        catch
        {
            return ("Environment", "Git", false, "Not installed");
        }
    }

    private static IEnumerable<(string Category, string Check, bool Passed, string Details)> CheckProjectStructure()
    {
        var solutionFile = Directory.GetFiles(".", "*.sln").FirstOrDefault();
        yield return ("Project Structure", "Solution file", solutionFile != null, solutionFile ?? "Not found");

        var slnPath = solutionFile ?? "RVR.Framework.sln";
        var expectedDirs = new[] { "src", "tests", "docs", "tools" };
        foreach (var dir in expectedDirs)
        {
            var exists = Directory.Exists(dir);
            yield return ("Project Structure", $"{dir}/ directory", exists, exists ? "Found" : "Missing");
        }

        var csprojFiles = Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories).Take(20).ToArray();
        yield return ("Project Structure", "C# projects", csprojFiles.Length > 0, $"{csprojFiles.Length} projects found");
    }

    private static IEnumerable<(string Category, string Check, bool Passed, string Details)> CheckDependencies()
    {
        var packagesConfig = Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories)
            .SelectMany(f => File.ReadAllLines(f))
            .Where(l => l.Contains("PackageReference"));

        var keyPackages = new[]
        {
            ("Spectre.Console", false),
            ("System.CommandLine", false),
            ("MediatR", false),
            ("FluentValidation", false)
        };

        var packagesText = string.Join("\n", packagesConfig);
        foreach (var (pkg, _) in keyPackages)
        {
            var found = packagesText.Contains(pkg);
            yield return ("Dependencies", pkg, found, found ? "Referenced" : "Missing");
        }
    }

    private static IEnumerable<(string Category, string Check, bool Passed, string Details)> CheckConfiguration()
    {
        var appSettings = Directory.GetFiles(".", "appsettings*.json", SearchOption.AllDirectories).FirstOrDefault();
        yield return ("Configuration", "appsettings.json", appSettings != null, appSettings ?? "Not found");

        var dockerCompose = Directory.GetFiles(".", "docker-compose*.yml").FirstOrDefault();
        yield return ("Configuration", "docker-compose.yml", dockerCompose != null, dockerCompose ?? "Not found");

        var githubActions = Directory.Exists(".github/workflows");
        yield return ("Configuration", "GitHub Actions", githubActions, githubActions ? "Configured" : "Not configured");

        var editorConfig = File.Exists(".editorconfig");
        yield return ("Configuration", ".editorconfig", editorConfig, editorConfig ? "Found" : "Missing");
    }

    private static void DisplayResults(IEnumerable<(string Category, string Check, bool Passed, string Details)> results)
    {
        var table = new Table();
        table.AddColumn(new TableColumn("Category").Centered());
        table.AddColumn(new TableColumn("Check").Centered());
        table.AddColumn(new TableColumn("Status").Centered());
        table.AddColumn(new TableColumn("Details").LeftAligned());

        foreach (var result in results.GroupBy(r => r.Category))
        {
            table.AddRow(
                $"[cyan]{result.Key}[/]",
                result.First().Check,
                result.First().Passed ? "[green]✓[/]" : "[red]✗[/]",
                result.First().Details
            );

            foreach (var item in result.Skip(1))
            {
                table.AddRow("", item.Check, item.Passed ? "[green]✓[/]" : "[red]✗[/]", item.Details);
            }
        }

        AnsiConsole.Write(table);
    }
}
