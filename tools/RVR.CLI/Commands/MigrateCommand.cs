using System.Diagnostics;
using System.Text;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides enhanced database migration management using Entity Framework Core tools.
/// Supports generate, apply, list, and rollback operations.
/// </summary>
public static class MigrateCommand
{
    /// <summary>
    /// Legacy entry point - applies migrations (kept for backward compat).
    /// </summary>
    public static async Task ExecuteAsync()
    {
        await ApplyAsync();
    }

    /// <summary>
    /// Generates a new migration with the given name.
    /// Runs: dotnet ef migrations add {name}
    /// </summary>
    public static async Task GenerateAsync(string name)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating migration:[/] {0}", name);

        var (dbContextProject, startupProject) = DetectProjects();
        AnsiConsole.MarkupLine("[grey]DbContext project:[/]  {0}", dbContextProject);
        AnsiConsole.MarkupLine("[grey]Startup project:[/]   {0}", startupProject);

        var args = $"ef migrations add {name} --project {dbContextProject} --startup-project {startupProject}";

        var exitCode = await RunDotnetCommandAsync("Adding migration...", args);

        if (exitCode == 0)
            AnsiConsole.MarkupLine("[bold green]Migration '{0}' created successfully![/]", name);
        else
            AnsiConsole.MarkupLine("[bold red]Migration generation failed (exit code {0}).[/]", exitCode);
    }

    /// <summary>
    /// Applies all pending migrations.
    /// Runs: dotnet ef database update
    /// </summary>
    public static async Task ApplyAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Applying database migrations...[/]");

        var (dbContextProject, startupProject) = DetectProjects();
        AnsiConsole.MarkupLine("[grey]DbContext project:[/]  {0}", dbContextProject);
        AnsiConsole.MarkupLine("[grey]Startup project:[/]   {0}", startupProject);

        var args = $"ef database update --project {dbContextProject} --startup-project {startupProject}";

        var exitCode = await RunDotnetCommandAsync("Applying migrations...", args);

        if (exitCode == 0)
            AnsiConsole.MarkupLine("[bold green]Database updated successfully![/]");
        else
            AnsiConsole.MarkupLine("[bold red]Database update failed (exit code {0}).[/]", exitCode);
    }

    /// <summary>
    /// Lists all migrations and their status.
    /// Runs: dotnet ef migrations list
    /// </summary>
    public static async Task ListAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Listing migrations...[/]");

        var (dbContextProject, startupProject) = DetectProjects();
        var args = $"ef migrations list --project {dbContextProject} --startup-project {startupProject}";

        var exitCode = await RunDotnetCommandAsync("Fetching migration list...", args);

        if (exitCode != 0)
            AnsiConsole.MarkupLine("[bold red]Failed to list migrations (exit code {0}).[/]", exitCode);
    }

    /// <summary>
    /// Rolls back to the previous migration.
    /// Runs: dotnet ef database update {previous-migration}
    /// First retrieves the migration list, then targets the second-to-last one.
    /// </summary>
    public static async Task RollbackAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Rolling back to previous migration...[/]");

        var (dbContextProject, startupProject) = DetectProjects();

        // Get migration list to find the previous migration
        var listArgs = $"ef migrations list --project {dbContextProject} --startup-project {startupProject} --no-color";

        string output;
        try
        {
            output = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Retrieving migration list...", async _ =>
                {
                    return await RunDotnetCaptureOutputAsync(listArgs);
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Failed to retrieve migration list:[/] {0}", ex.Message);
            return;
        }

        // Parse migrations from output - each line that isn't blank/header is a migration name
        var migrations = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) &&
                        !l.StartsWith("Build", StringComparison.OrdinalIgnoreCase) &&
                        !l.StartsWith("info:", StringComparison.OrdinalIgnoreCase) &&
                        !l.StartsWith("warn:", StringComparison.OrdinalIgnoreCase) &&
                        !l.StartsWith("Done.", StringComparison.OrdinalIgnoreCase))
            .Select(l => l.TrimEnd(" (Pending)".ToCharArray()))
            .ToList();

        if (migrations.Count < 2)
        {
            if (migrations.Count == 1)
            {
                AnsiConsole.MarkupLine("[yellow]Only one migration exists. Rolling back to initial state (migration 0)...[/]");
                var resetArgs = $"ef database update 0 --project {dbContextProject} --startup-project {startupProject}";
                var resetExit = await RunDotnetCommandAsync("Rolling back...", resetArgs);
                if (resetExit == 0)
                    AnsiConsole.MarkupLine("[bold green]Rolled back to initial state![/]");
                else
                    AnsiConsole.MarkupLine("[bold red]Rollback failed (exit code {0}).[/]", resetExit);
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No migrations found to roll back.[/]");
            }
            return;
        }

        var targetMigration = migrations[^2];
        AnsiConsole.MarkupLine("[grey]Target migration:[/] {0}", targetMigration);

        var rollbackArgs = $"ef database update {targetMigration} --project {dbContextProject} --startup-project {startupProject}";
        var exitCode = await RunDotnetCommandAsync("Rolling back...", rollbackArgs);

        if (exitCode == 0)
            AnsiConsole.MarkupLine("[bold green]Rolled back to '{0}' successfully![/]", targetMigration);
        else
            AnsiConsole.MarkupLine("[bold red]Rollback failed (exit code {0}).[/]", exitCode);
    }

    #region Helpers

    /// <summary>
    /// Auto-detects the DbContext project and startup project from the solution directory.
    /// </summary>
    private static (string dbContextProject, string startupProject) DetectProjects()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var srcDir = Path.Combine(currentDir, "src");

        string dbContextProject = "src/RVR.Framework.Infrastructure";
        string startupProject = "src/RVR.Framework.Api";

        if (!Directory.Exists(srcDir))
            return (dbContextProject, startupProject);

        // Look for Infrastructure project (usually holds DbContext)
        var infraDir = Directory.GetDirectories(srcDir, "*.Infrastructure").FirstOrDefault()
                    ?? Directory.GetDirectories(srcDir, "*.Persistence").FirstOrDefault()
                    ?? Directory.GetDirectories(srcDir, "*.Data").FirstOrDefault();

        if (infraDir != null)
            dbContextProject = $"src/{Path.GetFileName(infraDir)}";

        // Look for API / Web startup project
        var apiDir = Directory.GetDirectories(srcDir, "*.Api").FirstOrDefault()
                  ?? Directory.GetDirectories(srcDir, "*.Web").FirstOrDefault()
                  ?? Directory.GetDirectories(srcDir, "*.Server").FirstOrDefault();

        if (apiDir != null)
            startupProject = $"src/{Path.GetFileName(apiDir)}";

        return (dbContextProject, startupProject);
    }

    /// <summary>
    /// Runs a dotnet CLI command with Spectre.Console status spinner, showing live output.
    /// </summary>
    private static async Task<int> RunDotnetCommandAsync(string statusMessage, string arguments)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(statusMessage, async _ =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                        AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(e.Data));
                    }
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                        AnsiConsole.MarkupLine("[red]{0}[/]", Markup.Escape(e.Data));
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                return process.ExitCode;
            });
    }

    /// <summary>
    /// Runs a dotnet CLI command and captures its stdout as a string.
    /// </summary>
    private static async Task<string> RunDotnetCaptureOutputAsync(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }

    #endregion
}
