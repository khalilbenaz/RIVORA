using Spectre.Console;
using OpenAI.Chat;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using System.ClientModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

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
    public static async Task ExecuteAsync(string provider, string model, string? apiKey)
    {
        AnsiConsole.Write(new FigletText("AI Chat").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Interactive chat with LLM[/]" + Environment.NewLine);

        _provider = provider.ToLower();
        _model = string.IsNullOrWhiteSpace(model) ? GetDefaultModel(_provider) : model;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable(GetApiKeyEnvVar(_provider));

        if (string.IsNullOrWhiteSpace(_apiKey) && (_provider == "ollama" || _provider == "kilo"))
        {
            _apiKey = "local-dummy-key";
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            AnsiConsole.MarkupLine("[bold red]Error:[/] API key not provided.");
            AnsiConsole.MarkupLine("Set " + GetApiKeyEnvVar(_provider) + " environment variable or use --api-key option.");
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]Provider:[/] " + _provider);
        AnsiConsole.MarkupLine("[bold cyan]Model:[/] " + _model);
        if (_provider == "ollama" || _provider == "kilo")
        {
            var endpoint = _provider == "ollama"
                ? (Environment.GetEnvironmentVariable("OLLAMA_API_URL") ?? "http://localhost:11434/v1")
                : (Environment.GetEnvironmentVariable("KILO_API_URL") ?? "http://localhost:8080/v1");
            AnsiConsole.MarkupLine("[bold cyan]Endpoint:[/] " + endpoint);
        }
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

            conversationHistory.Add(("user", input));

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Thinking...", async ctx =>
                {
                    try
                    {
                        var response = await SendMessageAsync(conversationHistory);
                        conversationHistory.Add(("assistant", response));

                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold blue]Assistant:[/]");
                        AnsiConsole.MarkupLine("[white]" + Markup.Escape(response) + "[/]");
                        AnsiConsole.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine("[bold red]Error:[/] " + Markup.Escape(ex.Message));
                        conversationHistory.RemoveAt(conversationHistory.Count - 1);
                    }
                });
        }
    }

    private static string GetDefaultModel(string provider) => provider switch
    {
        "claude" => AnthropicModels.Claude35Sonnet,
        "openai" => "gpt-4o",
        "ollama" => "llama3",
        "kilo" => "kilo-model",
        _ => "gpt-4o"
    };

    private static string GetApiKeyEnvVar(string provider) => provider switch
    {
        "claude" => "ANTHROPIC_API_KEY",
        "openai" => "OPENAI_API_KEY",
        "ollama" => "OLLAMA_API_KEY",
        "kilo" => "KILO_API_KEY",
        _ => "OPENAI_API_KEY"
    };

    private static async Task<string> SendMessageAsync(List<(string Role, string Content)> history)
    {
        var systemPrompt = "You are a helpful software engineering assistant working inside the KBA.Framework CLI. Be concise and precise in your technical answers.";

        if (_provider == "claude")
        {
            var client = new AnthropicClient(_apiKey);
            var messages = history.Select(h => new Message()
            {
                Role = h.Role == "user" ? RoleType.User : RoleType.Assistant,
                Content = new List<ContentBase> { new TextContent() { Text = h.Content } }
            }).ToList();

            var parameters = new MessageParameters()
            {
                Messages = messages,
                MaxTokens = 2048,
                Model = _model,
                System = new List<SystemMessage>() { new SystemMessage(systemPrompt) }
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            var textContent = response.Message.Content[0] as TextContent;
            return textContent?.Text ?? "";
        }
        else if (_provider == "ollama" || _provider == "kilo")
        {
            var endpointUrl = _provider == "ollama"
                ? (Environment.GetEnvironmentVariable("OLLAMA_API_URL") ?? "http://localhost:11434/v1")
                : (Environment.GetEnvironmentVariable("KILO_API_URL") ?? "http://localhost:8080/v1");

            var clientOptions = new OpenAI.OpenAIClientOptions { Endpoint = new Uri(endpointUrl) };
            var client = new ChatClient(_model, new ApiKeyCredential(_apiKey!), clientOptions);

            var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };
            foreach (var h in history)
            {
                if (h.Role == "user")
                    messages.Add(new UserChatMessage(h.Content));
                else
                    messages.Add(new AssistantChatMessage(h.Content));
            }

            var completion = await client.CompleteChatAsync(messages);
            return completion.Value.Content[0].Text;
        }
        else // OpenAI
        {
            var client = new ChatClient(_model, new ApiKeyCredential(_apiKey!));
            var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };

            foreach (var h in history)
            {
                if (h.Role == "user")
                    messages.Add(new UserChatMessage(h.Content));
                else
                    messages.Add(new AssistantChatMessage(h.Content));
            }

            var completion = await client.CompleteChatAsync(messages);
            return completion.Value.Content[0].Text;
        }
    }
}
