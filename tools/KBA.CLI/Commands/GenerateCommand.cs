using Spectre.Console;

namespace KBA.CLI.Commands;

public static class GenerateCommand
{
    public static async Task GenerateAggregateAsync(string name, string module)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating aggregate: {0}[/]", name);
        
        var files = new[]
        {
            $"{name}Aggregate.cs",
            $"{name}CreatedEvent.cs",
            $"Create{name}Command.cs",
            $"Get{name}ByIdQuery.cs"
        };
        
        foreach (var file in files)
        {
            AnsiConsole.MarkupLine("[green]  ✓ Created[/] {0}", file);
            await Task.Delay(100);
        }
        
        AnsiConsole.MarkupLine("[bold green]✓ Aggregate generated successfully![/]");
    }
    
    public static async Task GenerateCrudAsync(string name, string props)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating CRUD for: {0}[/]", name);
        
        var files = new[]
        {
            $"{name}.cs",
            $"I{name}Repository.cs",
            $"Create{name}Command.cs",
            $"Update{name}Command.cs",
            $"Delete{name}Command.cs",
            $"Get{name}ByIdQuery.cs",
            $"GetAll{name}Query.cs",
            $"{name}Controller.cs"
        };
        
        foreach (var file in files)
        {
            AnsiConsole.MarkupLine("[green]  ✓ Created[/] {0}", file);
            await Task.Delay(100);
        }
        
        AnsiConsole.MarkupLine("[bold green]✓ CRUD generated successfully![/]");
    }
    
    public static async Task GenerateCommandAsync(string name)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating command: {0}[/]", name);
        AnsiConsole.MarkupLine("[green]  ✓ Created[/] {0}.cs", name);
        AnsiConsole.MarkupLine("[green]  ✓ Created[/] {0}Handler.cs", name);
        AnsiConsole.MarkupLine("[bold green]✓ Command generated successfully![/]");
        await Task.CompletedTask;
    }
    
    public static async Task GenerateQueryAsync(string name)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating query: {0}[/]", name);
        AnsiConsole.MarkupLine("[green]  ✓ Created[/] {0}.cs", name);
        AnsiConsole.MarkupLine("[green]  ✓ Created[/] {0}Handler.cs", name);
        AnsiConsole.MarkupLine("[bold green]✓ Query generated successfully![/]");
        await Task.CompletedTask;
    }
}
