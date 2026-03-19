using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides functionality to manage environments and secrets for RIVORA projects.
/// </summary>
public static class EnvCommand
{
    /// <summary>
    /// Lists available environments.
    /// </summary>
    public static async Task ListAsync()
    {
        AnsiConsole.Write(new FigletText("RVR Env").Color(Color.Yellow));
        AnsiConsole.MarkupLine("[grey]Environment management for RIVORA projects[/]" + Environment.NewLine);

        var currentDir = Directory.GetCurrentDirectory();
        var environments = DetectEnvironments(currentDir);

        if (environments.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No environment-specific appsettings files found.[/]");
            return;
        }

        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var table = new Table();
        table.AddColumn("Environment");
        table.AddColumn("File");
        table.AddColumn("Status");

        foreach (var env in environments)
        {
            var isCurrent = string.Equals(env.Name, currentEnv, StringComparison.OrdinalIgnoreCase);
            var status = isCurrent ? "[green]● Active[/]" : "[grey]○[/]";
            table.AddRow(
                isCurrent ? $"[bold green]{env.Name}[/]" : $"[cyan]{env.Name}",
                $"[grey]{env.RelativePath}[/]",
                status
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Active environment: [cyan]{currentEnv}[/] (from ASPNETCORE_ENVIRONMENT)[/]");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets a configuration value.
    /// </summary>
    public static async Task GetAsync(string key)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var settingsFile = FindAppSettings(currentDir, currentEnv);

        if (settingsFile == null)
        {
            AnsiConsole.MarkupLine($"[red]No appsettings.{currentEnv}.json found.[/]");
            return;
        }

        var content = await File.ReadAllTextAsync(settingsFile);
        var json = JsonNode.Parse(content);
        if (json == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to parse appsettings file.[/]");
            return;
        }

        var value = GetNestedValue(json, key);
        if (value != null)
        {
            AnsiConsole.MarkupLine($"[cyan]{key}[/] = [green]{value}[/]");
        }
        else
        {
            // Also check base appsettings.json
            var baseFile = Path.Combine(currentDir, "appsettings.json");
            if (File.Exists(baseFile))
            {
                var baseContent = await File.ReadAllTextAsync(baseFile);
                var baseJson = JsonNode.Parse(baseContent);
                value = GetNestedValue(baseJson, key);
                if (value != null)
                {
                    AnsiConsole.MarkupLine($"[cyan]{key}[/] = [green]{value}[/] [grey](from appsettings.json)[/]");
                    return;
                }
            }
            AnsiConsole.MarkupLine($"[yellow]Key '{key}' not found in {currentEnv} environment.[/]");
        }
    }

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    public static async Task SetAsync(string key, string value)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var settingsFile = FindAppSettings(currentDir, currentEnv);

        if (settingsFile == null)
        {
            // Create environment-specific settings file
            settingsFile = Path.Combine(currentDir, $"appsettings.{currentEnv}.json");
            await File.WriteAllTextAsync(settingsFile, "{}");
        }

        var content = await File.ReadAllTextAsync(settingsFile);
        var json = JsonNode.Parse(content) as JsonObject ?? new JsonObject();

        SetNestedValue(json, key, value);

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(settingsFile, json.ToJsonString(options));

        AnsiConsole.MarkupLine($"[green]✓[/] Set [cyan]{key}[/] = [green]{value}[/] in appsettings.{currentEnv}.json");
    }

    /// <summary>
    /// Removes a configuration key.
    /// </summary>
    public static async Task RemoveAsync(string key)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var settingsFile = FindAppSettings(currentDir, currentEnv);

        if (settingsFile == null)
        {
            AnsiConsole.MarkupLine($"[yellow]No appsettings.{currentEnv}.json found.[/]");
            return;
        }

        var content = await File.ReadAllTextAsync(settingsFile);
        var json = JsonNode.Parse(content) as JsonObject;
        if (json == null) return;

        if (RemoveNestedValue(json, key))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(settingsFile, json.ToJsonString(options));
            AnsiConsole.MarkupLine($"[green]✓[/] Removed [cyan]{key}[/] from appsettings.{currentEnv}.json");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]Key '{key}' not found.[/]");
        }
    }

    /// <summary>
    /// Switches the active environment.
    /// </summary>
    public static async Task SwitchAsync(string environment)
    {
        // Set via launchSettings.json
        var currentDir = Directory.GetCurrentDirectory();
        var launchSettingsPath = FindLaunchSettings(currentDir);

        if (launchSettingsPath != null)
        {
            var content = await File.ReadAllTextAsync(launchSettingsPath);
            var updated = Regex.Replace(
                content,
                @"""ASPNETCORE_ENVIRONMENT""\s*:\s*""[^""]*""",
                $@"""ASPNETCORE_ENVIRONMENT"": ""{environment}"""
            );
            await File.WriteAllTextAsync(launchSettingsPath, updated);
            AnsiConsole.MarkupLine($"[green]✓[/] Updated launchSettings.json to [cyan]{environment}[/]");
        }

        // Also set for current process
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        AnsiConsole.MarkupLine($"[green]✓[/] Environment switched to [cyan]{environment}[/]");
        AnsiConsole.MarkupLine("[grey]Note: Restart your application for the change to take effect.[/]");
    }

    /// <summary>
    /// Compares two environments.
    /// </summary>
    public static async Task DiffAsync(string env1, string env2)
    {
        AnsiConsole.Write(new FigletText("Env Diff").Color(Color.Yellow));

        var currentDir = Directory.GetCurrentDirectory();
        var file1 = FindAppSettings(currentDir, env1);
        var file2 = FindAppSettings(currentDir, env2);

        if (file1 == null)
        {
            AnsiConsole.MarkupLine($"[red]No appsettings found for '{env1}'.[/]");
            return;
        }
        if (file2 == null)
        {
            AnsiConsole.MarkupLine($"[red]No appsettings found for '{env2}'.[/]");
            return;
        }

        var content1 = await File.ReadAllTextAsync(file1);
        var content2 = await File.ReadAllTextAsync(file2);

        var flat1 = FlattenJson(JsonNode.Parse(content1));
        var flat2 = FlattenJson(JsonNode.Parse(content2));

        var allKeys = flat1.Keys.Union(flat2.Keys).OrderBy(k => k).ToList();

        var table = new Table();
        table.AddColumn("Key");
        table.AddColumn($"[cyan]{env1}[/]");
        table.AddColumn($"[cyan]{env2}[/]");
        table.AddColumn("Status");

        var differences = 0;
        foreach (var key in allKeys)
        {
            var has1 = flat1.TryGetValue(key, out var val1);
            var has2 = flat2.TryGetValue(key, out var val2);

            string status;
            if (!has1)
            {
                status = "[green]+ Added[/]";
                differences++;
            }
            else if (!has2)
            {
                status = "[red]- Missing[/]";
                differences++;
            }
            else if (val1 != val2)
            {
                status = "[yellow]~ Changed[/]";
                differences++;
            }
            else
            {
                continue; // Skip identical values
            }

            // Mask sensitive values
            var display1 = MaskSensitive(key, val1 ?? "[grey]—[/]");
            var display2 = MaskSensitive(key, val2 ?? "[grey]—[/]");

            table.AddRow($"[cyan]{key}[/]", display1, display2, status);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{differences} difference(s) found between {env1} and {env2}.[/]");
    }

    /// <summary>
    /// Initializes .NET User Secrets for the project.
    /// </summary>
    public static async Task SecretsInitAsync()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var apiProject = Directory.GetFiles(currentDir, "*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains(".Api", StringComparison.OrdinalIgnoreCase))
            ?? Directory.GetFiles(currentDir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();

        if (apiProject == null)
        {
            AnsiConsole.MarkupLine("[red]No .csproj file found.[/]");
            return;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"user-secrets init --project \"{apiProject}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = currentDir
        };

        using var process = System.Diagnostics.Process.Start(psi);
        if (process != null)
        {
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                AnsiConsole.MarkupLine("[green]✓[/] User Secrets initialized");
                if (!string.IsNullOrEmpty(output))
                    AnsiConsole.MarkupLine($"[grey]{output.Trim()}[/]");
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                AnsiConsole.MarkupLine($"[yellow]{error.Trim()}[/]");
            }
        }
    }

    /// <summary>
    /// Sets a secret value via .NET User Secrets.
    /// </summary>
    public static async Task SecretsSetAsync(string key, string value)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var apiProject = Directory.GetFiles(currentDir, "*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains(".Api", StringComparison.OrdinalIgnoreCase))
            ?? Directory.GetFiles(currentDir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();

        if (apiProject == null)
        {
            AnsiConsole.MarkupLine("[red]No .csproj file found.[/]");
            return;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"user-secrets set \"{key}\" \"{value}\" --project \"{apiProject}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = currentDir
        };

        using var process = System.Diagnostics.Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
                AnsiConsole.MarkupLine($"[green]✓[/] Secret [cyan]{key}[/] set successfully");
            else
                AnsiConsole.MarkupLine("[red]Failed to set secret. Run 'rvr env secrets init' first.[/]");
        }
    }

    /// <summary>
    /// Exports configuration to a file.
    /// </summary>
    public static async Task ExportAsync(string format = "dotenv")
    {
        var currentDir = Directory.GetCurrentDirectory();
        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Merge base + environment settings
        var merged = new Dictionary<string, string>();

        var baseFile = Path.Combine(currentDir, "appsettings.json");
        if (File.Exists(baseFile))
        {
            var baseContent = await File.ReadAllTextAsync(baseFile);
            foreach (var kv in FlattenJson(JsonNode.Parse(baseContent)))
                merged[kv.Key] = kv.Value;
        }

        var envFile = FindAppSettings(currentDir, currentEnv);
        if (envFile != null)
        {
            var envContent = await File.ReadAllTextAsync(envFile);
            foreach (var kv in FlattenJson(JsonNode.Parse(envContent)))
                merged[kv.Key] = kv.Value;
        }

        if (merged.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No configuration found to export.[/]");
            return;
        }

        string outputFile;
        string content;

        switch (format.ToLowerInvariant())
        {
            case "dotenv":
            case ".env":
                outputFile = ".env";
                content = string.Join(Environment.NewLine,
                    merged.OrderBy(kv => kv.Key)
                        .Select(kv => $"{kv.Key.Replace(":", "__")}={MaskSensitiveValue(kv.Key, kv.Value)}"));
                break;
            case "json":
                outputFile = $"env.{currentEnv.ToLower()}.json";
                content = JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true });
                break;
            case "yaml":
            case "yml":
                outputFile = $"env.{currentEnv.ToLower()}.yml";
                content = string.Join(Environment.NewLine,
                    merged.OrderBy(kv => kv.Key)
                        .Select(kv => $"{kv.Key.Replace(":", ".")}: \"{MaskSensitiveValue(kv.Key, kv.Value)}\""));
                break;
            default:
                AnsiConsole.MarkupLine($"[red]Unsupported format: {format}. Use dotenv, json, or yaml.[/]");
                return;
        }

        await File.WriteAllTextAsync(Path.Combine(currentDir, outputFile), content);
        AnsiConsole.MarkupLine($"[green]✓[/] Exported {merged.Count} keys to [cyan]{outputFile}[/]");
        AnsiConsole.MarkupLine("[yellow]⚠ Review the file and fill in masked sensitive values before use.[/]");
    }

    /// <summary>
    /// Imports configuration from a .env file.
    /// </summary>
    public static async Task ImportAsync(string file)
    {
        if (!File.Exists(file))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {file}[/]");
            return;
        }

        var currentDir = Directory.GetCurrentDirectory();
        var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var settingsFile = FindAppSettings(currentDir, currentEnv)
            ?? Path.Combine(currentDir, $"appsettings.{currentEnv}.json");

        var json = File.Exists(settingsFile)
            ? JsonNode.Parse(await File.ReadAllTextAsync(settingsFile)) as JsonObject ?? new JsonObject()
            : new JsonObject();

        var lines = await File.ReadAllLinesAsync(file);
        var imported = 0;
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;

            var eqIdx = trimmed.IndexOf('=');
            if (eqIdx <= 0) continue;

            var key = trimmed[..eqIdx].Trim().Replace("__", ":");
            var value = trimmed[(eqIdx + 1)..].Trim().Trim('"');

            SetNestedValue(json, key, value);
            imported++;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(settingsFile, json.ToJsonString(options));

        AnsiConsole.MarkupLine($"[green]✓[/] Imported {imported} keys into appsettings.{currentEnv}.json");
    }

    // Helper methods

    private static List<EnvironmentInfo> DetectEnvironments(string dir)
    {
        var envs = new List<EnvironmentInfo>();

        // Search for appsettings.*.json files
        var settingsFiles = Directory.GetFiles(dir, "appsettings*.json", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        foreach (var file in settingsFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var match = Regex.Match(fileName, @"appsettings\.(.+)");
            if (match.Success)
            {
                envs.Add(new EnvironmentInfo
                {
                    Name = match.Groups[1].Value,
                    FilePath = file,
                    RelativePath = Path.GetRelativePath(dir, file)
                });
            }
        }

        return envs.DistinctBy(e => e.Name).OrderBy(e => e.Name).ToList();
    }

    private static string? FindAppSettings(string dir, string env)
    {
        var files = Directory.GetFiles(dir, $"appsettings.{env}.json", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();
        return files.FirstOrDefault();
    }

    private static string? FindLaunchSettings(string dir)
    {
        return Directory.GetFiles(dir, "launchSettings.json", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains("Properties"));
    }

    private static string? GetNestedValue(JsonNode? node, string key)
    {
        if (node == null) return null;

        var parts = key.Split(new[] { ":", "__" }, StringSplitOptions.RemoveEmptyEntries);
        JsonNode? current = node;
        foreach (var part in parts)
        {
            current = current?[part];
            if (current == null) return null;
        }
        return current?.ToString();
    }

    private static void SetNestedValue(JsonObject root, string key, string value)
    {
        var parts = key.Split(new[] { ":", "__" }, StringSplitOptions.RemoveEmptyEntries);
        var current = root;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (current[parts[i]] is not JsonObject child)
            {
                child = new JsonObject();
                current[parts[i]] = child;
            }
            current = child;
        }

        current[parts[^1]] = value;
    }

    private static bool RemoveNestedValue(JsonObject root, string key)
    {
        var parts = key.Split(new[] { ":", "__" }, StringSplitOptions.RemoveEmptyEntries);
        JsonObject current = root;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (current[parts[i]] is not JsonObject child)
                return false;
            current = child;
        }

        return current.Remove(parts[^1]);
    }

    private static Dictionary<string, string> FlattenJson(JsonNode? node, string prefix = "")
    {
        var result = new Dictionary<string, string>();
        if (node == null) return result;

        if (node is JsonObject obj)
        {
            foreach (var prop in obj)
            {
                var newPrefix = string.IsNullOrEmpty(prefix) ? prop.Key : $"{prefix}:{prop.Key}";
                foreach (var kv in FlattenJson(prop.Value, newPrefix))
                    result[kv.Key] = kv.Value;
            }
        }
        else
        {
            result[prefix] = node.ToString();
        }

        return result;
    }

    private static string MaskSensitive(string key, string value)
    {
        var sensitivePatterns = new[] { "password", "secret", "key", "token", "connectionstring" };
        var lowerKey = key.ToLowerInvariant();

        if (sensitivePatterns.Any(p => lowerKey.Contains(p)))
            return "[red]****[/]";

        return value;
    }

    private static string MaskSensitiveValue(string key, string value)
    {
        var sensitivePatterns = new[] { "password", "secret", "key", "token", "connectionstring" };
        var lowerKey = key.ToLowerInvariant();

        if (sensitivePatterns.Any(p => lowerKey.Contains(p)))
            return "<REPLACE_ME>";

        return value;
    }

    private class EnvironmentInfo
    {
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string RelativePath { get; set; } = "";
    }
}
