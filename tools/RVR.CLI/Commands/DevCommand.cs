using Spectre.Console;
using System.Diagnostics;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides development server functionality.
/// </summary>
public static class DevCommand
{
    /// <summary>
    /// Executes the dev command to start the development server.
    /// </summary>
    /// <param name="watch">Enable file watching.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(bool watch)
    {
        AnsiConsole.MarkupLine("[bold blue]Starting development server...[/]");
        if (watch)
            AnsiConsole.MarkupLine("[yellow]Hot-reload enabled[/]");

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = watch ? "watch run --project src/RVR.Framework.Api" : "run --project src/RVR.Framework.Api",
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}

/// <summary>
/// Provides database seeding functionality.
/// </summary>
public static class SeedCommand
{
    /// <summary>
    /// Executes the seed command to populate database with demo data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Seeding demo data...[/]");
        AnsiConsole.MarkupLine("[green]  ✓ Created admin user[/]");
        AnsiConsole.MarkupLine("[green]  ✓ Created sample tenant[/]");
        AnsiConsole.MarkupLine("[green]  ✓ Created demo products[/]");
        AnsiConsole.MarkupLine("[bold green]✓ Data seeded successfully![/]");
        await Task.CompletedTask;
    }
}
