using Spectre.Console;

namespace KBA.CLI.Commands;

/// <summary>
/// Provides interactive chat functionality with LLM providers (OpenAI/Claude).
/// </summary>
public static class AiChatCommand
{
    private static string? _apiKey;
    private static string _provider = "openai";
    private static string _model = "gpt-4o";

    /// <summary>
    /// Executes the AI chat command.
    /// </summary>
    /// <param name="provider">The LLM provider (openai, claude).</param>
    /// <param name="model">The model to use.</param>
    /// <param name="apiKey">The API key for the provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(string provider, string model, string? apiKey)
    {
        AnsiConsole.Write(new FigletText("AI Chat").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Interactive chat with LLM[/]" + Environment.NewLine);

        _provider = provider.ToLower();
        _model = string.IsNullOrWhiteSpace(model) ? GetDefaultModel(_provider) : model;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable(GetApiKeyEnvVar(_provider));

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            AnsiConsole.MarkupLine("[bold red]Error:[/] API key not provided.");
            AnsiConsole.MarkupLine("Set " + GetApiKeyEnvVar(_provider) + " environment variable or use --api-key option.");
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]Provider:[/] " + _provider);
        AnsiConsole.MarkupLine("[bold cyan]Model:[/] " + _model);
        AnsiConsole.MarkupLine("[grey]Type your message (or 'quit' to exit)[/]" + Environment.NewLine);

        var conversationHistory = new List<(string Role, string Content)>();

        while (true)
        {
            AnsiConsole.Markup("[bold green]>[/] ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.Equals("quit", StringComparison.OrdinalIgnoreCase) || 
                input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
                break;
            }

            if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                conversationHistory.Clear();
                AnsiConsole.MarkupLine("[yellow]Conversation cleared.[/]");
                continue;
            }

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Thinking...", async ctx =>
                {
                    try
                    {
                        var response = await SendMessageAsync(input, conversationHistory);
                        conversationHistory.Add(("user", input));
                        conversationHistory.Add(("assistant", response));

                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold blue]Assistant:[/]");
                        AnsiConsole.MarkupLine("[white]" + response + "[/]");
                        AnsiConsole.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine("[bold red]Error:[/] " + ex.Message);
                    }
                });
        }

        await Task.CompletedTask;
    }

    private static string GetDefaultModel(string provider) => provider switch
    {
        "claude" => "claude-sonnet-4-5-20250929",
        "openai" => "gpt-4o",
        _ => "gpt-4o"
    };

    private static string GetApiKeyEnvVar(string provider) => provider switch
    {
        "claude" => "ANTHROPIC_API_KEY",
        "openai" => "OPENAI_API_KEY",
        _ => "OPENAI_API_KEY"
    };

    private static async Task<string> SendMessageAsync(string message, List<(string Role, string Content)> history)
    {
        // Simulated response for demo - in production, integrate with actual API
        await Task.Delay(500); // Simulate API call
        
        // This is a placeholder - actual implementation would call OpenAI/Claude API
        return "[Simulated response from " + _model + "] I received your message: \"" + message + "\". In production mode, this would connect to the " + _provider + " API.";
    }
}
