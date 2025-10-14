namespace RVR.CLI.Commands;

using Spectre.Console;
using RVR.CLI.Services;

/// <summary>
/// AI-assisted domain design command. Starts an interactive session where developers
/// describe their domain in natural language and the AI generates entity definitions,
/// value objects, aggregate roots, domain events, and CQRS scaffolding.
/// </summary>
public static class AiDesignCommand
{
    public static async Task ExecuteAsync(string provider, string? apiKey, string? model)
    {
        var llmClient = LlmClientFactory.Create(provider, apiKey);

        if (!llmClient.IsAvailable)
        {
            AnsiConsole.MarkupLine("[red]LLM provider not available. Check your API key or provider configuration.[/]");
            return;
        }

        AnsiConsole.Write(new FigletText("RVR Domain Designer").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[cyan]Decrivez votre domaine metier en langage naturel.[/]");
        AnsiConsole.MarkupLine("[grey]Tapez 'generate' pour generer le code, 'diagram' pour le diagramme, 'quit' pour quitter.[/]");
        AnsiConsole.WriteLine();

        var systemPrompt = @"You are a DDD expert. When the user describes a domain:
1. Identify entities, value objects, aggregate roots
2. Define relationships (1:1, 1:N, N:M)
3. Suggest domain events for key state changes
4. Output in structured JSON format with:
   - entities: [{name, properties: [{name, type, isRequired}], isAggregateRoot, valueObjects: [...]}]
   - relationships: [{from, to, type, navigationType}]
   - domainEvents: [{name, trigger, properties}]
When user says 'generate', output C# code for each entity.
When user says 'diagram', output Mermaid class diagram.
Always respond in the same language as the user.";

        var conversationHistory = new List<(string role, string content)>();

        while (true)
        {
            var input = AnsiConsole.Prompt(new TextPrompt<string>("[green]>[/] "));

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            if (input.Equals("generate", StringComparison.OrdinalIgnoreCase))
                input = "Generate C# entity classes with proper DDD patterns (AggregateRoot, Entity, ValueObject base classes from RVR.Framework.Core). Use file-scoped namespaces, .NET 9, and include domain events.";

            if (input.Equals("diagram", StringComparison.OrdinalIgnoreCase))
                input = "Generate a Mermaid class diagram showing all entities, value objects, and their relationships.";

            conversationHistory.Add(("user", input));

            var fullUserPrompt = string.Join("\n", conversationHistory.Select(h => $"{h.role}: {h.content}"));

            var response = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Thinking...", async ctx =>
                {
                    return await llmClient.CompleteAsync(systemPrompt, fullUserPrompt);
                });

            conversationHistory.Add(("assistant", response));

            // Render with a panel if it contains code blocks, plain text otherwise
            if (response.Contains("```"))
            {
                AnsiConsole.Write(new Panel(Markup.Escape(response))
                    .Header("[cyan]Domain Model[/]")
                    .Border(BoxBorder.Rounded));
            }
            else
            {
                AnsiConsole.MarkupLine($"[white]{Markup.Escape(response)}[/]");
            }

            AnsiConsole.WriteLine();
        }
    }
}
