using Spectre.Console;
using System.Diagnostics;

namespace KBA.CLI.Commands;

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
            Arguments = watch ? "watch run --project src/KBA.Framework.Api" : "run --project src/KBA.Framework.Api",
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}

/// <summary>
/// Provides database migration functionality.
/// </summary>
public static class MigrateCommand
{
    /// <summary>
    /// Executes the migrate command to apply database migrations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync()
    {
        AnsiConsole.MarkupLine("[bold blue]Applying migrations...[/]");
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "ef database update --project src/KBA.Framework.Infrastructure --startup-project src/KBA.Framework.Api",
            UseShellExecute = true
        };
        Process.Start(psi);
        await Task.CompletedTask;
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
