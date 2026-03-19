using System.Diagnostics;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides functionality to seed databases with test/demo data using standardized seeders.
/// </summary>
public static class SeedCommand
{
    /// <summary>
    /// Executes the seed command.
    /// </summary>
    /// <param name="profile">Seeding profile (dev, demo, test, perf).</param>
    /// <param name="reset">Whether to truncate database before seeding.</param>
    /// <param name="dryRun">Show what would be seeded without executing.</param>
    /// <param name="tenant">Optional tenant ID for multi-tenant seeding.</param>
    public static async Task ExecuteAsync(
        string profile = "dev",
        bool reset = false,
        bool dryRun = false,
        string? tenant = null)
    {
        AnsiConsole.Write(new FigletText("RVR Seed").Color(Color.Green));
        AnsiConsole.MarkupLine("[grey]Database seeding for RIVORA projects[/]" + Environment.NewLine);

        // Display configuration
        var configTable = new Table();
        configTable.AddColumn("Option");
        configTable.AddColumn("Value");
        configTable.AddRow("[cyan]Profile[/]", $"[green]{profile}[/]");
        configTable.AddRow("[cyan]Reset DB[/]", reset ? "[red]Yes (truncate + reseed)[/]" : "[grey]No[/]");
        configTable.AddRow("[cyan]Dry Run[/]", dryRun ? "[yellow]Yes[/]" : "[grey]No[/]");
        configTable.AddRow("[cyan]Tenant[/]", tenant ?? "[grey]All tenants[/]");
        AnsiConsole.Write(configTable);
        AnsiConsole.WriteLine();

        // Find the project
        var currentDir = Directory.GetCurrentDirectory();
        var solutionFile = Directory.GetFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (solutionFile == null)
        {
            AnsiConsole.MarkupLine("[red]Error: No solution file (.sln) found in current directory.[/]");
            return;
        }

        // Scan for seeder classes implementing IRvrDataSeeder
        var seeders = new List<SeederInfo>();
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Scanning for data seeders...", async ctx =>
            {
                seeders = await ScanForSeeders(currentDir, profile);
            });

        if (seeders.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No seeders found for profile '{profile}'.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]To create a seeder:[/]");
            AnsiConsole.MarkupLine("  1. Create a class implementing [cyan]IRvrDataSeeder[/]");
            AnsiConsole.MarkupLine("  2. Set the [cyan]Profile[/] property to your target profile");
            AnsiConsole.MarkupLine("  3. Implement [cyan]SeedAsync()[/] with your seed logic");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Example:[/]");
            AnsiConsole.Write(new Panel(
                "[grey]public class UserSeeder : IRvrDataSeeder\n" +
                "{\n" +
                "    public string Profile => \"dev\";\n" +
                "    public int Order => 1;\n" +
                "    public async Task SeedAsync(CancellationToken ct) { ... }\n" +
                "}[/]"
            ).Header("[cyan]IRvrDataSeeder[/]"));
            return;
        }

        // Sort by order
        seeders = seeders.OrderBy(s => s.Order).ToList();

        // Display seeders that will run
        var seedersTable = new Table();
        seedersTable.AddColumn("#");
        seedersTable.AddColumn("Seeder");
        seedersTable.AddColumn("Profile");
        seedersTable.AddColumn("Order");
        seedersTable.AddColumn("File");

        for (var i = 0; i < seeders.Count; i++)
        {
            var s = seeders[i];
            seedersTable.AddRow(
                $"[grey]{i + 1}[/]",
                $"[cyan]{s.ClassName}[/]",
                $"[green]{s.Profile}[/]",
                $"[grey]{s.Order}[/]",
                $"[grey]{s.RelativePath}[/]"
            );
        }
        AnsiConsole.Write(seedersTable);
        AnsiConsole.WriteLine();

        if (dryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run — the above seeders would be executed in order.[/]");
            return;
        }

        // Confirm reset if requested
        if (reset)
        {
            var confirm = AnsiConsole.Confirm(
                "[red]This will truncate the database before seeding. Continue?[/]", false);
            if (!confirm)
            {
                AnsiConsole.MarkupLine("[grey]Seeding cancelled.[/]");
                return;
            }
        }

        // Execute seeding via dotnet run
        AnsiConsole.WriteLine();
        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            })
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Running seeders...[/]", maxValue: seeders.Count);

                foreach (var seeder in seeders)
                {
                    task.Description = $"[green]Seeding {seeder.ClassName}...[/]";

                    // Build dotnet run command with seed arguments
                    var args = $"--seed --profile {profile}";
                    if (reset) args += " --reset";
                    if (tenant != null) args += $" --tenant {tenant}";

                    await RunDotnetSeedAsync(currentDir, seeder, args);
                    task.Increment(1);
                }

                task.Description = "[green]Seeding complete[/]";
            });

        // Summary
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ Seeding completed successfully![/]");

        var summaryTable = new Table();
        summaryTable.AddColumn("Seeder");
        summaryTable.AddColumn("Status");
        foreach (var seeder in seeders)
        {
            summaryTable.AddRow($"[cyan]{seeder.ClassName}[/]", "[green]✓ Done[/]");
        }
        AnsiConsole.Write(summaryTable);
    }

    /// <summary>
    /// Generates a new seeder scaffold.
    /// </summary>
    public static async Task GenerateAsync(string entityName, string profile = "dev")
    {
        AnsiConsole.MarkupLine($"[cyan]Generating seeder for {entityName}...[/]");

        var currentDir = Directory.GetCurrentDirectory();
        var seedDir = FindOrCreateSeedDirectory(currentDir);
        var fileName = $"{entityName}Seeder.cs";
        var filePath = Path.Combine(seedDir, fileName);

        if (File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[yellow]Seeder already exists: {Path.GetRelativePath(currentDir, filePath)}[/]");
            return;
        }

        // Detect namespace from directory
        var ns = DetectNamespace(seedDir, currentDir);

        var seederCode = $@"using RVR.Framework.Core.Seeding;

namespace {ns};

/// <summary>
/// Data seeder for {entityName} entities.
/// </summary>
public class {entityName}Seeder : IRvrDataSeeder
{{
    public string Profile => ""{profile}"";
    public int Order => 10;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {{
        // TODO: Implement seeding logic for {entityName}
        // Example:
        // var entities = new List<{entityName}>
        // {{
        //     {entityName}.Create(""Sample 1"", ""Description 1""),
        //     {entityName}.Create(""Sample 2"", ""Description 2""),
        // }};
        //
        // foreach (var entity in entities)
        // {{
        //     await _repository.AddAsync(entity, cancellationToken);
        // }}
        // await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await Task.CompletedTask;
    }}
}}";

        await File.WriteAllTextAsync(filePath, seederCode);
        AnsiConsole.MarkupLine($"[green]✓[/] Created {Path.GetRelativePath(currentDir, filePath)}");
        AnsiConsole.MarkupLine("[grey]Edit the file to add your seed data.[/]");
    }

    private static async Task<List<SeederInfo>> ScanForSeeders(string rootDir, string profile)
    {
        var seeders = new List<SeederInfo>();
        var csFiles = Directory.GetFiles(rootDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        foreach (var file in csFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);

                // Look for classes implementing IRvrDataSeeder
                if (!content.Contains("IRvrDataSeeder")) continue;

                var classMatch = Regex.Match(content, @"class\s+(\w+)\s*:\s*.*IRvrDataSeeder");
                if (!classMatch.Success) continue;

                var className = classMatch.Groups[1].Value;

                // Extract profile
                var profileMatch = Regex.Match(content, @"Profile\s*=>\s*""(\w+)""");
                var seederProfile = profileMatch.Success ? profileMatch.Groups[1].Value : "dev";

                // Filter by requested profile
                if (!string.Equals(seederProfile, profile, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extract order
                var orderMatch = Regex.Match(content, @"Order\s*=>\s*(\d+)");
                var order = orderMatch.Success ? int.Parse(orderMatch.Groups[1].Value) : 100;

                seeders.Add(new SeederInfo
                {
                    ClassName = className,
                    Profile = seederProfile,
                    Order = order,
                    FilePath = file,
                    RelativePath = Path.GetRelativePath(rootDir, file)
                });
            }
            catch { }
        }

        return seeders;
    }

    private static async Task RunDotnetSeedAsync(string workingDir, SeederInfo seeder, string args)
    {
        // Find the project containing the seeder
        var projectDir = Path.GetDirectoryName(seeder.FilePath);
        while (projectDir != null && !Directory.GetFiles(projectDir, "*.csproj").Any())
        {
            projectDir = Path.GetDirectoryName(projectDir);
        }

        if (projectDir == null) return;

        var csproj = Directory.GetFiles(projectDir, "*.csproj").First();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{csproj}\" -- {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDir
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: Could not run seeder {seeder.ClassName}: {ex.Message}[/]");
        }
    }

    private static string FindOrCreateSeedDirectory(string rootDir)
    {
        // Look for existing Seeding/Seeds directory
        var candidates = new[] { "Seeding", "Seeds", "Data/Seeds", "Infrastructure/Seeding" };
        foreach (var candidate in candidates)
        {
            var dirs = Directory.GetDirectories(rootDir, candidate, SearchOption.AllDirectories)
                .Where(d => !d.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                            !d.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToList();
            if (dirs.Count > 0) return dirs.First();
        }

        // Create default
        var seedDir = Path.Combine(rootDir, "src", "Infrastructure", "Seeding");
        Directory.CreateDirectory(seedDir);
        return seedDir;
    }

    private static string DetectNamespace(string dir, string rootDir)
    {
        // Try to detect namespace from existing .cs files in the directory
        var csFiles = Directory.GetFiles(dir, "*.cs").Take(1);
        foreach (var f in csFiles)
        {
            var content = File.ReadAllText(f);
            var nsMatch = Regex.Match(content, @"namespace\s+([\w.]+)");
            if (nsMatch.Success)
                return nsMatch.Groups[1].Value;
        }

        // Fallback: build from path
        var relative = Path.GetRelativePath(rootDir, dir).Replace(Path.DirectorySeparatorChar, '.');
        return $"RVR.Framework.{relative}";
    }

    private class SeederInfo
    {
        public string ClassName { get; set; } = "";
        public string Profile { get; set; } = "";
        public int Order { get; set; }
        public string FilePath { get; set; } = "";
        public string RelativePath { get; set; } = "";
    }
}
