using Spectre.Console;

namespace KBA.CLI.Commands;

public static class NewCommand
{
    public static async Task ExecuteAsync(string name, string template, string tenancy)
    {
        AnsiConsole.MarkupLine("[bold blue]Creating new KBA Framework project: {0}[/]", name);
        AnsiConsole.MarkupLine("[bold blue]Template: {0} | Tenancy: {1}[/]", template, tenancy);

        AnsiConsole.MarkupLine("[yellow]Creating project structure...[/]");
        Directory.CreateDirectory(name);
        Directory.CreateDirectory(Path.Combine(name, "src"));
        Directory.CreateDirectory(Path.Combine(name, "tests"));
        Directory.CreateDirectory(Path.Combine(name, "docs"));

        AnsiConsole.MarkupLine("[bold green]✓ Project created successfully![/]");
        AnsiConsole.MarkupLine("[yellow]Next steps:[/]");
        AnsiConsole.MarkupLine("  cd {0}", name);
        AnsiConsole.MarkupLine("  dotnet restore");
        AnsiConsole.MarkupLine("  kba dev");

        await Task.CompletedTask;
    }
}
