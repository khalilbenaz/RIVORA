using Spectre.Console;
using System.Text;

namespace KBA.CLI.Commands;

/// <summary>
/// Provides shell completion script generation for bash, zsh, and PowerShell.
/// </summary>
public static class CompletionCommand
{
    /// <summary>
    /// Generates and outputs shell completion script.
    /// </summary>
    /// <param name="shell">Target shell (bash, zsh, pwsh).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(string shell)
    {
        AnsiConsole.Write(new FigletText("Completion").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Shell completion script[/]" + Environment.NewLine);

        var script = shell.ToLower() switch
        {
            "bash" => GenerateBashCompletion(),
            "zsh" => GenerateZshCompletion(),
            "pwsh" or "powershell" => GeneratePowerShellCompletion(),
            _ => GenerateBashCompletion()
        };

        AnsiConsole.MarkupLine("[bold cyan]Shell:[/] " + shell);
        AnsiConsole.MarkupLine("[bold cyan]Installation:[/]" + Environment.NewLine);

        var installInstructions = shell.ToLower() switch
        {
            "bash" => "  # Add to ~/.bashrc:\n  source <(kba completion bash)\n  \n  # Or save to file:\n  kba completion bash > /etc/bash_completion.d/kba",
            "zsh" => "  # Add to ~/.zshrc:\n  source <(kba completion zsh)\n  \n  # Or save to file:\n  kba completion zsh > /usr/local/share/zsh/site-functions/_kba",
            "pwsh" or "powershell" => "  # Add to $PROFILE:\n  kba completion pwsh | Out-String | Invoke-Expression\n  \n  # Or save to file:\n  kba completion pwsh > $PROFILE",
            _ => string.Empty
        };

        AnsiConsole.MarkupLine("[yellow]" + installInstructions + "[/]");
        AnsiConsole.WriteLine();

        // Show script preview
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(script)
        {
            Header = new PanelHeader("[cyan]" + shell + " completion script[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0, 1, 0)
        });

        await Task.CompletedTask;
    }

    private static string GenerateBashCompletion()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Bash completion script for kba");
        sb.AppendLine("# Add to ~/.bashrc or /etc/bash_completion.d/kba");
        sb.AppendLine();
        sb.AppendLine("_kba_completion() {");
        sb.AppendLine("    local cur=\"${COMP_WORDS[COMP_CWORD]}\"");
        sb.AppendLine("    local prev=\"${COMP_WORDS[COMP_CWORD-1]}\"");
        sb.AppendLine("    local commands=\"new generate gen g dev migrate seed doctor ai add-module benchmark completion\"");
        sb.AppendLine("    local ai_commands=\"chat generate review\"");
        sb.AppendLine("    local generate_commands=\"aggregate crud command query\"");
        sb.AppendLine("    ");
        sb.AppendLine("    case \"${prev}\" in");
        sb.AppendLine("        kba)");
        sb.AppendLine("            COMPREPLY=($(compgen -W \"${commands}\" -- \"${cur}\"))");
        sb.AppendLine("            return 0");
        sb.AppendLine("            ;;");
        sb.AppendLine("        ai)");
        sb.AppendLine("            COMPREPLY=($(compgen -W \"${ai_commands}\" -- \"${cur}\"))");
        sb.AppendLine("            return 0");
        sb.AppendLine("            ;;");
        sb.AppendLine("        generate|gen|g)");
        sb.AppendLine("            COMPREPLY=($(compgen -W \"${generate_commands}\" -- \"${cur}\"))");
        sb.AppendLine("            return 0");
        sb.AppendLine("            ;;");
        sb.AppendLine("        *)");
        sb.AppendLine("            COMPREPLY=()");
        sb.AppendLine("            return 0");
        sb.AppendLine("            ;;");
        sb.AppendLine("    esac");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("complete -F _kba_completion kba");
        return sb.ToString();
    }

    private static string GenerateZshCompletion()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Zsh completion script for kba");
        sb.AppendLine("# Add to ~/.zshrc or /usr/local/share/zsh/site-functions/_kba");
        sb.AppendLine();
        sb.AppendLine("#compdef kba");
        sb.AppendLine();
        sb.AppendLine("_kba() {");
        sb.AppendLine("    local -a commands");
        sb.AppendLine("    local -a ai_commands");
        sb.AppendLine("    local -a generate_commands");
        sb.AppendLine("    ");
        sb.AppendLine("    commands=(");
        sb.AppendLine("        'new:Create a new KBA Framework project'");
        sb.AppendLine("        'generate:Generate code (aliases: gen, g)'");
        sb.AppendLine("        'dev:Start development server'");
        sb.AppendLine("        'migrate:Apply database migrations'");
        sb.AppendLine("        'seed:Seed database with demo data'");
        sb.AppendLine("        'doctor:Diagnose project issues'");
        sb.AppendLine("        'ai:AI commands'");
        sb.AppendLine("        'add-module:Add a complete module'");
        sb.AppendLine("        'benchmark:Run load tests'");
        sb.AppendLine("        'completion:Generate shell completion'");
        sb.AppendLine("    )");
        sb.AppendLine("    ");
        sb.AppendLine("    ai_commands=(");
        sb.AppendLine("        'chat:Interactive chat with LLM'");
        sb.AppendLine("        'generate:Generate code with AI'");
        sb.AppendLine("        'review:Review code with AI'");
        sb.AppendLine("    )");
        sb.AppendLine("    ");
        sb.AppendLine("    generate_commands=(");
        sb.AppendLine("        'aggregate:Generate an aggregate root'");
        sb.AppendLine("        'crud:Generate CRUD operations'");
        sb.AppendLine("        'command:Generate a CQRS command'");
        sb.AppendLine("        'query:Generate a CQRS query'");
        sb.AppendLine("    )");
        sb.AppendLine("    ");
        sb.AppendLine("    _arguments '1: :->command' '*: :->args'");
        sb.AppendLine("    ");
        sb.AppendLine("    case $state in");
        sb.AppendLine("        command)");
        sb.AppendLine("            _describe 'commands' commands");
        sb.AppendLine("            ;;");
        sb.AppendLine("        args)");
        sb.AppendLine("            case $words[1] in");
        sb.AppendLine("                ai)");
        sb.AppendLine("                    _describe 'ai commands' ai_commands");
        sb.AppendLine("                    ;;");
        sb.AppendLine("                generate|gen|g)");
        sb.AppendLine("                    _describe 'generate commands' generate_commands");
        sb.AppendLine("                    ;;");
        sb.AppendLine("            esac");
        sb.AppendLine("            ;;");
        sb.AppendLine("    esac");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("_kba");
        return sb.ToString();
    }

    private static string GeneratePowerShellCompletion()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# PowerShell completion script for kba");
        sb.AppendLine("# Add to $PROFILE or run: kba completion pwsh | Out-String | Invoke-Expression");
        sb.AppendLine();
        sb.AppendLine("Register-ArgumentCompleter -Native -CommandName kba -ScriptBlock {");
        sb.AppendLine("    param($wordToComplete, $commandAst, $cursorPosition)");
        sb.AppendLine("    ");
        sb.AppendLine("    $commands = @(");
        sb.AppendLine("        'new', 'generate', 'gen', 'g', 'dev', 'migrate', 'seed',");
        sb.AppendLine("        'doctor', 'ai', 'add-module', 'benchmark', 'completion'");
        sb.AppendLine("    )");
        sb.AppendLine("    ");
        sb.AppendLine("    $aiCommands = @('chat', 'generate', 'review')");
        sb.AppendLine("    $generateCommands = @('aggregate', 'crud', 'command', 'query')");
        sb.AppendLine("    ");
        sb.AppendLine("    $commandElements = $commandAst.Extent.Text.Split(' ')");
        sb.AppendLine("    ");
        sb.AppendLine("    if ($commandElements.Count -eq 2) {");
        sb.AppendLine("        $commands | Where-Object { $_ -like \"$wordToComplete*\" } | ForEach-Object {");
        sb.AppendLine("            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("    elseif ($commandElements[1] -eq 'ai') {");
        sb.AppendLine("        $aiCommands | Where-Object { $_ -like \"$wordToComplete*\" } | ForEach-Object {");
        sb.AppendLine("            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("    elseif ($commandElements[1] -in @('generate', 'gen', 'g')) {");
        sb.AppendLine("        $generateCommands | Where-Object { $_ -like \"$wordToComplete*\" } | ForEach-Object {");
        sb.AppendLine("            [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
