using System.Text;
using System.Xml.Linq;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Interactive wizard to create a complete RIVORA Framework solution from scratch.
/// Supports both interactive mode (prompts) and non-interactive mode (CLI flags).
/// </summary>
public static class NewCommand
{
    /// <summary>
    /// Writes a file, creating the parent directory if it doesn't exist.
    /// </summary>
    private static void WriteFile(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        WriteFile(path, content);
    }

    /// <summary>
    /// Executes the new command in non-interactive mode.
    /// </summary>
    public static async Task ExecuteAsync(string name, string template, string tenancy)
    {
        // If called with just a name and default template, launch interactive wizard
        if (template == "minimal" && tenancy == "row" && !Console.IsInputRedirected)
        {
            await ExecuteInteractiveAsync(name);
            return;
        }

        // Non-interactive mode with flags
        var profile = new SolutionProfile
        {
            Name = name,
            Namespace = name,
            AppType = template switch
            {
                "saas-starter" => "api-blazor",
                "ai-rag" => "api",
                "microservices" => "microservice",
                _ => "api"
            },
            Database = "postgresql",
            TenancyMode = tenancy switch
            {
                "schema" => "schema",
                "database" => "database",
                "none" => "none",
                _ => "shared"
            },
        };

        // Set defaults based on template
        if (template == "saas-starter")
        {
            profile.Modules.AddRange(new[] { "Caching", "HealthChecks", "Notifications", "Jobs.Hangfire" });
            profile.Security.AddRange(new[] { "jwt", "identity-pro" });
            profile.IncludeBilling = true;
        }
        else if (template == "ai-rag")
        {
            profile.Modules.AddRange(new[] { "Caching", "HealthChecks" });
            profile.Security.Add("jwt");
            profile.AiMode = "all";
        }
        else
        {
            profile.Modules.AddRange(new[] { "Caching", "HealthChecks" });
            profile.Security.Add("jwt");
        }

        profile.DevOps.AddRange(new[] { "ci", "docker-compose", "gitignore" });

        await GenerateSolution(profile);
    }

    /// <summary>
    /// Executes the new command with full CLI flags (non-interactive).
    /// </summary>
    public static async Task ExecuteWithFlagsAsync(
        string name,
        string type,
        string db,
        string modules,
        string security,
        string multitenancy,
        string devops,
        string ai,
        string? output)
    {
        var profile = new SolutionProfile
        {
            Name = name,
            Namespace = name,
            OutputDir = output ?? $"./{name}",
            AppType = type,
            Database = db,
            TenancyMode = multitenancy,
            AiMode = ai
        };

        if (!string.IsNullOrEmpty(modules))
            profile.Modules.AddRange(modules.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        if (!string.IsNullOrEmpty(security))
            profile.Security.AddRange(security.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        if (!string.IsNullOrEmpty(devops))
            profile.DevOps.AddRange(devops.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        await GenerateSolution(profile);
    }

    /// <summary>
    /// Interactive wizard mode using Spectre.Console prompts.
    /// </summary>
    private static async Task ExecuteInteractiveAsync(string? suggestedName)
    {
        AnsiConsole.Write(new FigletText("RVR New").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Interactive solution wizard for RIVORA Framework[/]" + Environment.NewLine);

        var profile = new SolutionProfile();

        // === Step 1: General Info ===
        AnsiConsole.Write(new Rule("[cyan]Step 1 — General Information[/]").LeftJustified());

        profile.Name = AnsiConsole.Ask("[cyan]Solution name:[/]", suggestedName ?? "MyApp");
        profile.Namespace = AnsiConsole.Ask("[cyan]Root namespace:[/]", profile.Name);
        profile.OutputDir = AnsiConsole.Ask("[cyan]Output directory:[/]", $"./{profile.Name}");

        // === Step 2: App Type ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 2 — Application Type[/]").LeftJustified());

        profile.AppType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Application type:[/]")
                .AddChoices("api", "api-blazor", "microservice", "worker"));

        // === Step 3: Database ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 3 — Database[/]").LeftJustified());

        profile.Database = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Database provider:[/]")
                .AddChoices("postgresql", "sqlserver", "mysql", "sqlite", "mongodb", "cosmosdb", "none"));

        // === Step 4: Modules ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 4 — Modules[/]").LeftJustified());

        profile.Modules = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[cyan]Select modules to include:[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoiceGroup("Infrastructure", "Caching", "HealthChecks", "Storage", "Resilience", "Idempotency")
                .AddChoiceGroup("Messaging", "Events", "Messaging", "Notifications", "Webhooks")
                .AddChoiceGroup("Jobs", "Jobs.Hangfire", "Jobs.Quartz")
                .AddChoiceGroup("Architecture", "EventSourcing", "Saga")
                .AddChoiceGroup("Features", "FeatureManagement", "Localization.Dynamic", "Search", "Workflow")
                .AddChoiceGroup("Real-Time", "RealTime"));

        // === Step 5: Security ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 5 — Security[/]").LeftJustified());

        profile.Security = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[cyan]Security options:[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices("jwt", "apikeys", "identity-pro", "privacy-gdpr", "ip-allowlist", "rate-limiting"));

        // === Step 6: Multi-tenancy ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 6 — Multi-tenancy[/]").LeftJustified());

        profile.TenancyMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Multi-tenancy mode:[/]")
                .AddChoices("none", "shared", "schema", "database"));

        if (profile.TenancyMode != "none")
        {
            profile.IncludeBilling = AnsiConsole.Confirm("[cyan]Include Billing module (Stripe)?[/]", false);
            profile.IncludeSaaS = AnsiConsole.Confirm("[cyan]Include SaaS module?[/]", false);
        }

        // === Step 7: Integrations ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 7 — Integrations[/]").LeftJustified());

        profile.Integrations = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[cyan]Integrations:[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices("Email", "SMS", "Webhooks", "Export", "GraphQL", "Client"));

        // === Step 8: AI ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 8 — AI[/]").LeftJustified());

        profile.AiMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]AI module:[/]")
                .AddChoices("none", "ai-base", "ai-naturalquery", "ai-agents", "all"));

        // === Step 9: Observability ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 9 — Observability[/]").LeftJustified());

        profile.Observability = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[cyan]Observability:[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices("OpenTelemetry", "HealthChecks", "Grafana+Prometheus", "Seq", "Aspire"));

        // === Step 10: DevOps ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Step 10 — DevOps[/]").LeftJustified());

        profile.DevOps = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[cyan]Generate DevOps files:[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices("ci", "docker-compose", "dockerfile", "editorconfig", "gitignore"));

        // === Summary ===
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold cyan]Summary[/]").LeftJustified());

        var summaryTable = new Table().Border(TableBorder.Rounded);
        summaryTable.AddColumn("Option");
        summaryTable.AddColumn("Value");
        summaryTable.AddRow("[cyan]Name[/]", profile.Name);
        summaryTable.AddRow("[cyan]Namespace[/]", profile.Namespace);
        summaryTable.AddRow("[cyan]Output[/]", profile.OutputDir);
        summaryTable.AddRow("[cyan]Type[/]", profile.AppType);
        summaryTable.AddRow("[cyan]Database[/]", profile.Database);
        summaryTable.AddRow("[cyan]Modules[/]", profile.Modules.Count > 0 ? string.Join(", ", profile.Modules) : "[grey]None[/]");
        summaryTable.AddRow("[cyan]Security[/]", profile.Security.Count > 0 ? string.Join(", ", profile.Security) : "[grey]None[/]");
        summaryTable.AddRow("[cyan]Multi-tenancy[/]", profile.TenancyMode);
        summaryTable.AddRow("[cyan]Integrations[/]", profile.Integrations.Count > 0 ? string.Join(", ", profile.Integrations) : "[grey]None[/]");
        summaryTable.AddRow("[cyan]AI[/]", profile.AiMode);
        summaryTable.AddRow("[cyan]Observability[/]", profile.Observability.Count > 0 ? string.Join(", ", profile.Observability) : "[grey]None[/]");
        summaryTable.AddRow("[cyan]DevOps[/]", profile.DevOps.Count > 0 ? string.Join(", ", profile.DevOps) : "[grey]None[/]");
        AnsiConsole.Write(summaryTable);

        AnsiConsole.WriteLine();
        if (!AnsiConsole.Confirm("[bold green]Generate solution?[/]", true))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            return;
        }

        await GenerateSolution(profile);

        // Show equivalent CLI command
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Equivalent CLI command:[/]");
        AnsiConsole.MarkupLine($"[cyan]{GenerateCliCommand(profile)}[/]");
    }

    /// <summary>
    /// Generates the full RIVORA solution from a profile.
    /// </summary>
    public static async Task GenerateSolution(SolutionProfile profile)
    {
        AnsiConsole.WriteLine();
        var createdFiles = new List<string>();
        var rootDir = profile.OutputDir;

        await AnsiConsole.Progress()
            .AutoClear(false)
            .AutoRefresh(true)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            })
            .StartAsync(async ctx =>
            {
                // 1. Directory structure
                var structTask = ctx.AddTask("[cyan]Creating directory structure...[/]", maxValue: 100);
                createdFiles.AddRange(CreateDirectoryStructure(rootDir, profile));
                structTask.Increment(100);

                // 2. Solution file
                var slnTask = ctx.AddTask("[cyan]Generating solution file...[/]", maxValue: 100);
                createdFiles.Add(GenerateSolutionFile(rootDir, profile));
                slnTask.Increment(100);

                // 3. Directory.Build.props
                var buildTask = ctx.AddTask("[cyan]Generating build configuration...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateBuildProps(rootDir, profile));
                buildTask.Increment(100);

                // 4. Domain project
                var domainTask = ctx.AddTask("[cyan]Creating Domain layer...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateDomainProject(rootDir, profile));
                domainTask.Increment(100);

                // 5. Application project
                var appTask = ctx.AddTask("[cyan]Creating Application layer...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateApplicationProject(rootDir, profile));
                appTask.Increment(100);

                // 6. Infrastructure project
                var infraTask = ctx.AddTask("[cyan]Creating Infrastructure layer...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateInfrastructureProject(rootDir, profile));
                infraTask.Increment(100);

                // 7. API project
                var apiTask = ctx.AddTask("[cyan]Creating API layer...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateApiProject(rootDir, profile));
                apiTask.Increment(100);

                // 8. Program.cs
                var programTask = ctx.AddTask("[cyan]Generating Program.cs...[/]", maxValue: 100);
                createdFiles.Add(GenerateProgramCs(rootDir, profile));
                programTask.Increment(100);

                // 9. AppSettings
                var settingsTask = ctx.AddTask("[cyan]Generating appsettings...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateAppSettings(rootDir, profile));
                settingsTask.Increment(100);

                // 10. DevOps files
                var devopsTask = ctx.AddTask("[cyan]Generating DevOps files...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateDevOpsFiles(rootDir, profile));
                devopsTask.Increment(100);

                // 11. Tests project
                var testsTask = ctx.AddTask("[cyan]Creating test project...[/]", maxValue: 100);
                createdFiles.AddRange(GenerateTestProject(rootDir, profile));
                testsTask.Increment(100);

                await Task.CompletedTask;
            });

        // Results
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ Solution '{profile.Name}' created successfully![/]");
        AnsiConsole.MarkupLine($"[grey]{createdFiles.Count} files generated in {rootDir}[/]");

        AnsiConsole.WriteLine();
        var tree = new Tree($"[cyan]{profile.Name}/[/]");
        var srcNode = tree.AddNode("[cyan]src/[/]");
        srcNode.AddNode($"{profile.Name}.Domain/");
        srcNode.AddNode($"{profile.Name}.Application/");
        srcNode.AddNode($"{profile.Name}.Infrastructure/");
        srcNode.AddNode($"{profile.Name}.Api/");
        var testsNode = tree.AddNode("[cyan]tests/[/]");
        testsNode.AddNode($"{profile.Name}.Tests/");
        if (profile.DevOps.Count > 0)
        {
            var devopsNode = tree.AddNode("[cyan]devops/[/]");
            foreach (var d in profile.DevOps) devopsNode.AddNode(d);
        }
        AnsiConsole.Write(tree);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Next steps:[/]");
        AnsiConsole.MarkupLine($"  cd [cyan]{profile.OutputDir}[/]");
        AnsiConsole.MarkupLine("  dotnet restore");
        AnsiConsole.MarkupLine("  dotnet build");
        AnsiConsole.MarkupLine("  rvr dev");
    }

    // ===== Generation Methods =====

    private static List<string> CreateDirectoryStructure(string root, SolutionProfile profile)
    {
        var dirs = new[]
        {
            root,
            Path.Combine(root, "src"),
            Path.Combine(root, $"src/{profile.Name}.Domain/Entities"),
            Path.Combine(root, $"src/{profile.Name}.Domain/Events"),
            Path.Combine(root, $"src/{profile.Name}.Domain/Exceptions"),
            Path.Combine(root, $"src/{profile.Name}.Domain/Repositories"),
            Path.Combine(root, $"src/{profile.Name}.Application/Features"),
            Path.Combine(root, $"src/{profile.Name}.Application/Common"),
            Path.Combine(root, $"src/{profile.Name}.Infrastructure/Repositories"),
            Path.Combine(root, $"src/{profile.Name}.Infrastructure/Configurations"),
            Path.Combine(root, $"src/{profile.Name}.Api/Endpoints"),
            Path.Combine(root, "tests"),
            Path.Combine(root, $"tests/{profile.Name}.Tests"),
        };

        foreach (var dir in dirs)
            Directory.CreateDirectory(dir);

        return new List<string>();
    }

    private static string GenerateSolutionFile(string root, SolutionProfile profile)
    {
        var slnPath = Path.Combine(root, $"{profile.Name}.sln");
        var sb = new StringBuilder();
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");

        var projects = new[]
        {
            ($"{profile.Name}.Domain", $"src/{profile.Name}.Domain/{profile.Name}.Domain.csproj"),
            ($"{profile.Name}.Application", $"src/{profile.Name}.Application/{profile.Name}.Application.csproj"),
            ($"{profile.Name}.Infrastructure", $"src/{profile.Name}.Infrastructure/{profile.Name}.Infrastructure.csproj"),
            ($"{profile.Name}.Api", $"src/{profile.Name}.Api/{profile.Name}.Api.csproj"),
            ($"{profile.Name}.Tests", $"tests/{profile.Name}.Tests/{profile.Name}.Tests.csproj"),
        };

        foreach (var (name, path) in projects)
        {
            var guid = Guid.NewGuid().ToString("B").ToUpper();
            sb.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{name}\", \"{path}\", \"{guid}\"");
            sb.AppendLine("EndProject");
        }

        sb.AppendLine("Global");
        sb.AppendLine("EndGlobal");

        WriteFile(slnPath, sb.ToString());
        return slnPath;
    }

    private static List<string> GenerateBuildProps(string root, SolutionProfile profile)
    {
        var files = new List<string>();

        // Directory.Build.props
        var buildPropsPath = Path.Combine(root, "Directory.Build.props");
        WriteFile(buildPropsPath, $@"<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Version>1.0.0</Version>
    <RootNamespace>{profile.Namespace}</RootNamespace>
  </PropertyGroup>
</Project>");
        files.Add(buildPropsPath);

        // Directory.Packages.props
        var packagesPropsPath = Path.Combine(root, "Directory.Packages.props");
        var packages = new StringBuilder();
        packages.AppendLine(@"<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include=""MediatR"" Version=""12.4.1"" />
    <PackageVersion Include=""FluentValidation"" Version=""11.11.0"" />
    <PackageVersion Include=""Swashbuckle.AspNetCore"" Version=""7.2.0"" />");

        // Database packages
        if (profile.Database == "postgresql")
            packages.AppendLine(@"    <PackageVersion Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""9.0.4"" />");
        else if (profile.Database == "sqlserver")
            packages.AppendLine(@"    <PackageVersion Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""9.0.3"" />");
        else if (profile.Database == "mysql")
            packages.AppendLine(@"    <PackageVersion Include=""Pomelo.EntityFrameworkCore.MySql"" Version=""9.0.0"" />");
        else if (profile.Database == "sqlite")
            packages.AppendLine(@"    <PackageVersion Include=""Microsoft.EntityFrameworkCore.Sqlite"" Version=""9.0.3"" />");

        packages.AppendLine(@"    <PackageVersion Include=""Microsoft.EntityFrameworkCore"" Version=""9.0.3"" />
    <PackageVersion Include=""Microsoft.EntityFrameworkCore.Design"" Version=""9.0.3"" />
    <PackageVersion Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""9.0.3"" />
    <PackageVersion Include=""xunit"" Version=""2.9.3"" />
    <PackageVersion Include=""xunit.runner.visualstudio"" Version=""3.0.2"" />
    <PackageVersion Include=""Microsoft.NET.Test.Sdk"" Version=""17.12.0"" />
    <PackageVersion Include=""NSubstitute"" Version=""5.3.0"" />
    <PackageVersion Include=""FluentAssertions"" Version=""7.1.0"" />
  </ItemGroup>
</Project>");

        WriteFile(packagesPropsPath, packages.ToString());
        files.Add(packagesPropsPath);

        return files;
    }

    private static List<string> GenerateDomainProject(string root, SolutionProfile profile)
    {
        var files = new List<string>();
        var dir = Path.Combine(root, $"src/{profile.Name}.Domain");

        // .csproj
        var csproj = Path.Combine(dir, $"{profile.Name}.Domain.csproj");
        WriteFile(csproj, $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{profile.Namespace}.Domain</RootNamespace>
  </PropertyGroup>
</Project>");
        files.Add(csproj);

        // Sample entity
        WriteFile(Path.Combine(dir, "Entities/SampleEntity.cs"), $@"namespace {profile.Namespace}.Domain.Entities;

/// <summary>
/// Sample entity — replace with your domain entities.
/// </summary>
public class SampleEntity
{{
    public Guid Id {{ get; private set; }}
    public string Name {{ get; private set; }} = string.Empty;
    public string Description {{ get; private set; }} = string.Empty;
    public bool IsActive {{ get; private set; }} = true;
    public DateTime CreatedAt {{ get; private set; }}
    public DateTime UpdatedAt {{ get; private set; }}

    private SampleEntity() {{ }}

    public static SampleEntity Create(string name, string description)
    {{
        return new SampleEntity
        {{
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }};
    }}

    public void Update(string name, string description)
    {{
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }}
}}");
        files.Add(Path.Combine(dir, "Entities/SampleEntity.cs"));

        return files;
    }

    private static List<string> GenerateApplicationProject(string root, SolutionProfile profile)
    {
        var files = new List<string>();
        var dir = Path.Combine(root, $"src/{profile.Name}.Application");

        var csproj = Path.Combine(dir, $"{profile.Name}.Application.csproj");
        WriteFile(csproj, $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{profile.Namespace}.Application</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MediatR"" />
    <PackageReference Include=""FluentValidation"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{profile.Name}.Domain\{profile.Name}.Domain.csproj"" />
  </ItemGroup>
</Project>");
        files.Add(csproj);

        // DependencyInjection
        WriteFile(Path.Combine(dir, "DependencyInjection.cs"), $@"using Microsoft.Extensions.DependencyInjection;

namespace {profile.Namespace}.Application;

public static class DependencyInjection
{{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {{
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }}
}}");
        files.Add(Path.Combine(dir, "DependencyInjection.cs"));

        return files;
    }

    private static List<string> GenerateInfrastructureProject(string root, SolutionProfile profile)
    {
        var files = new List<string>();
        var dir = Path.Combine(root, $"src/{profile.Name}.Infrastructure");

        var dbPackage = profile.Database switch
        {
            "postgresql" => "<PackageReference Include=\"Npgsql.EntityFrameworkCore.PostgreSQL\" />",
            "sqlserver" => "<PackageReference Include=\"Microsoft.EntityFrameworkCore.SqlServer\" />",
            "mysql" => "<PackageReference Include=\"Pomelo.EntityFrameworkCore.MySql\" />",
            "sqlite" => "<PackageReference Include=\"Microsoft.EntityFrameworkCore.Sqlite\" />",
            _ => ""
        };

        var csproj = Path.Combine(dir, $"{profile.Name}.Infrastructure.csproj");
        WriteFile(csproj, $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{profile.Namespace}.Infrastructure</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" PrivateAssets=""all"" />
    {dbPackage}
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{profile.Name}.Domain\{profile.Name}.Domain.csproj"" />
    <ProjectReference Include=""..\{profile.Name}.Application\{profile.Name}.Application.csproj"" />
  </ItemGroup>
</Project>");
        files.Add(csproj);

        // AppDbContext
        WriteFile(Path.Combine(dir, "AppDbContext.cs"), $@"using {profile.Namespace}.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace {profile.Namespace}.Infrastructure;

public class AppDbContext : DbContext
{{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {{ }}

    public DbSet<SampleEntity> Samples => Set<SampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }}
}}");
        files.Add(Path.Combine(dir, "AppDbContext.cs"));

        // DependencyInjection
        WriteFile(Path.Combine(dir, "DependencyInjection.cs"), $@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {profile.Namespace}.Infrastructure;

public static class DependencyInjection
{{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {{
        services.AddDbContext<AppDbContext>(options =>
        {{
{GetDbContextConfig(profile)}
        }});

        return services;
    }}
}}");
        files.Add(Path.Combine(dir, "DependencyInjection.cs"));

        return files;
    }

    private static List<string> GenerateApiProject(string root, SolutionProfile profile)
    {
        var files = new List<string>();
        var dir = Path.Combine(root, $"src/{profile.Name}.Api");

        var apiPackages = new StringBuilder();
        apiPackages.AppendLine(@"    <PackageReference Include=""Swashbuckle.AspNetCore"" />");

        if (profile.Security.Contains("jwt"))
            apiPackages.AppendLine(@"    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" />");

        var csproj = Path.Combine(dir, $"{profile.Name}.Api.csproj");
        WriteFile(csproj, $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <RootNamespace>{profile.Namespace}.Api</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
{apiPackages}  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{profile.Name}.Application\{profile.Name}.Application.csproj"" />
    <ProjectReference Include=""..\{profile.Name}.Infrastructure\{profile.Name}.Infrastructure.csproj"" />
  </ItemGroup>
</Project>");
        files.Add(csproj);

        // Sample endpoint
        WriteFile(Path.Combine(dir, "Endpoints/SampleEndpoints.cs"), $@"namespace {profile.Namespace}.Api.Endpoints;

public static class SampleEndpoints
{{
    public static void MapSampleEndpoints(this WebApplication app)
    {{
        var group = app.MapGroup(""/api/samples"").WithTags(""Samples"");

        group.MapGet(""/"", () => Results.Ok(new {{ message = ""Hello from {profile.Name}!"", timestamp = DateTime.UtcNow }}))
            .WithName(""GetSamples"");

        group.MapGet(""/health"", () => Results.Ok(new {{ status = ""healthy"" }}))
            .WithName(""GetHealth"");
    }}
}}");
        files.Add(Path.Combine(dir, "Endpoints/SampleEndpoints.cs"));

        return files;
    }

    private static string GenerateProgramCs(string root, SolutionProfile profile)
    {
        var path = Path.Combine(root, $"src/{profile.Name}.Api/Program.cs");
        var sb = new StringBuilder();

        sb.AppendLine($"using {profile.Namespace}.Application;");
        sb.AppendLine($"using {profile.Namespace}.Infrastructure;");
        sb.AppendLine($"using {profile.Namespace}.Api.Endpoints;");
        sb.AppendLine();
        sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
        sb.AppendLine();
        sb.AppendLine("// === Services ===");
        sb.AppendLine("builder.Services.AddApplication();");
        sb.AppendLine("builder.Services.AddInfrastructure(builder.Configuration);");
        sb.AppendLine("builder.Services.AddEndpointsApiExplorer();");
        sb.AppendLine("builder.Services.AddSwaggerGen();");

        if (profile.Security.Contains("jwt"))
        {
            sb.AppendLine();
            sb.AppendLine("// Authentication");
            sb.AppendLine("builder.Services.AddAuthentication().AddJwtBearer();");
            sb.AppendLine("builder.Services.AddAuthorization();");
        }

        if (profile.Modules.Contains("Caching"))
        {
            sb.AppendLine();
            sb.AppendLine("// Caching");
            sb.AppendLine("builder.Services.AddMemoryCache();");
        }

        if (profile.Modules.Contains("HealthChecks") || profile.Observability.Contains("HealthChecks"))
        {
            sb.AppendLine();
            sb.AppendLine("// Health Checks");
            sb.AppendLine("builder.Services.AddHealthChecks();");
        }

        sb.AppendLine();
        sb.AppendLine("var app = builder.Build();");
        sb.AppendLine();
        sb.AppendLine("// === Middleware ===");
        sb.AppendLine("if (app.Environment.IsDevelopment())");
        sb.AppendLine("{");
        sb.AppendLine("    app.UseSwagger();");
        sb.AppendLine("    app.UseSwaggerUI();");
        sb.AppendLine("}");

        if (profile.Security.Contains("jwt"))
        {
            sb.AppendLine();
            sb.AppendLine("app.UseAuthentication();");
            sb.AppendLine("app.UseAuthorization();");
        }

        sb.AppendLine();
        sb.AppendLine("// === Endpoints ===");
        sb.AppendLine("app.MapSampleEndpoints();");

        if (profile.Modules.Contains("HealthChecks") || profile.Observability.Contains("HealthChecks"))
            sb.AppendLine("app.MapHealthChecks(\"/health\");");

        sb.AppendLine();
        sb.AppendLine("app.Run();");

        WriteFile(path, sb.ToString());
        return path;
    }

    private static List<string> GenerateAppSettings(string root, SolutionProfile profile)
    {
        var files = new List<string>();
        var apiDir = Path.Combine(root, $"src/{profile.Name}.Api");

        var connStr = profile.Database switch
        {
            "postgresql" => "Host=localhost;Database=" + profile.Name.ToLower() + "db;Username=postgres;Password=postgres",
            "sqlserver" => "Server=localhost;Database=" + profile.Name.ToLower() + "db;Trusted_Connection=True;TrustServerCertificate=True",
            "mysql" => "Server=localhost;Database=" + profile.Name.ToLower() + "db;User=root;Password=root",
            "sqlite" => "Data Source=" + profile.Name.ToLower() + ".db",
            _ => ""
        };

        var appSettings = $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }}
  }},
  ""AllowedHosts"": ""*"",
  ""ConnectionStrings"": {{
    ""DefaultConnection"": ""{connStr}""
  }}
}}";

        WriteFile(Path.Combine(apiDir, "appsettings.json"), appSettings);
        files.Add(Path.Combine(apiDir, "appsettings.json"));

        WriteFile(Path.Combine(apiDir, "appsettings.Development.json"), @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Debug"",
      ""Microsoft.AspNetCore"": ""Information""
    }
  }
}");
        files.Add(Path.Combine(apiDir, "appsettings.Development.json"));

        return files;
    }

    private static List<string> GenerateDevOpsFiles(string root, SolutionProfile profile)
    {
        var files = new List<string>();

        if (profile.DevOps.Contains("gitignore"))
        {
            var path = Path.Combine(root, ".gitignore");
            WriteFile(path, @"## .NET
bin/
obj/
*.user
*.suo
.vs/
*.DotSettings.user

## IDE
.idea/
.vscode/

## Build
artifacts/
publish/
nupkg/

## Environment
.env
*.pfx
appsettings.*.local.json
");
            files.Add(path);
        }

        if (profile.DevOps.Contains("editorconfig"))
        {
            var path = Path.Combine(root, ".editorconfig");
            WriteFile(path, @"root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{csproj,props,targets,xml}]
indent_size = 2

[*.{json,yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
");
            files.Add(path);
        }

        if (profile.DevOps.Contains("ci"))
        {
            var ciDir = Path.Combine(root, ".github/workflows");
            Directory.CreateDirectory(ciDir);
            var path = Path.Combine(ciDir, "ci.yml");
            WriteFile(path, $@"name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

permissions:
  contents: read

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release --verbosity normal
");
            files.Add(path);
        }

        if (profile.DevOps.Contains("dockerfile"))
        {
            var path = Path.Combine(root, "Dockerfile");
            WriteFile(path, $@"FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish src/{profile.Name}.Api/{profile.Name}.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [""dotnet"", ""{profile.Name}.Api.dll""]
");
            files.Add(path);
        }

        if (profile.DevOps.Contains("docker-compose"))
        {
            var path = Path.Combine(root, "docker-compose.dev.yml");
            var dbService = profile.Database switch
            {
                "postgresql" => @"
  db:
    image: postgres:16-alpine
    ports:
      - ""5432:5432""
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: " + profile.Name.ToLower() + @"db
    volumes:
      - db-data:/var/lib/postgresql/data",
                "sqlserver" => @"
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - ""1433:1433""
    environment:
      ACCEPT_EULA: ""Y""
      SA_PASSWORD: ""SuperSecret123!""
    volumes:
      - db-data:/var/opt/mssql",
                _ => ""
            };

            var additionalServices = new StringBuilder();
            if (profile.Modules.Contains("Caching"))
            {
                additionalServices.Append(@"
  redis:
    image: redis:alpine
    ports:
      - ""6379:6379""");
            }

            WriteFile(path, $@"services:{dbService}{additionalServices}

volumes:
  db-data:
");
            files.Add(path);
        }

        return files;
    }

    private static List<string> GenerateTestProject(string root, SolutionProfile profile)
    {
        var files = new List<string>();
        var dir = Path.Combine(root, $"tests/{profile.Name}.Tests");
        Directory.CreateDirectory(dir);

        var csproj = Path.Combine(dir, $"{profile.Name}.Tests.csproj");
        WriteFile(csproj, $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <RootNamespace>{profile.Namespace}.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" />
    <PackageReference Include=""xunit"" />
    <PackageReference Include=""xunit.runner.visualstudio"" PrivateAssets=""all"" />
    <PackageReference Include=""NSubstitute"" />
    <PackageReference Include=""FluentAssertions"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\..\src\{profile.Name}.Domain\{profile.Name}.Domain.csproj"" />
    <ProjectReference Include=""..\..\src\{profile.Name}.Application\{profile.Name}.Application.csproj"" />
  </ItemGroup>
</Project>");
        files.Add(csproj);

        // Sample test
        WriteFile(Path.Combine(dir, "SampleEntityTests.cs"), $@"using {profile.Namespace}.Domain.Entities;
using FluentAssertions;

namespace {profile.Namespace}.Tests;

public class SampleEntityTests
{{
    [Fact]
    public void Create_ShouldInitializeProperties()
    {{
        var entity = SampleEntity.Create(""Test"", ""Description"");

        entity.Name.Should().Be(""Test"");
        entity.Description.Should().Be(""Description"");
        entity.IsActive.Should().BeTrue();
        entity.Id.Should().NotBeEmpty();
    }}

    [Fact]
    public void Update_ShouldModifyProperties()
    {{
        var entity = SampleEntity.Create(""Original"", ""Desc"");
        entity.Update(""Updated"", ""New Desc"");

        entity.Name.Should().Be(""Updated"");
        entity.Description.Should().Be(""New Desc"");
    }}
}}");
        files.Add(Path.Combine(dir, "SampleEntityTests.cs"));

        return files;
    }

    private static string GetDbContextConfig(SolutionProfile profile) => profile.Database switch
    {
        "postgresql" => "            options.UseNpgsql(configuration.GetConnectionString(\"DefaultConnection\"));",
        "sqlserver" => "            options.UseSqlServer(configuration.GetConnectionString(\"DefaultConnection\"));",
        "mysql" => "            options.UseMySql(configuration.GetConnectionString(\"DefaultConnection\"), ServerVersion.AutoDetect(configuration.GetConnectionString(\"DefaultConnection\")));",
        "sqlite" => "            options.UseSqlite(configuration.GetConnectionString(\"DefaultConnection\"));",
        _ => "            // No database configured"
    };

    private static string GenerateCliCommand(SolutionProfile profile)
    {
        var cmd = new StringBuilder($"rvr new {profile.Name}");
        cmd.Append($" --type {profile.AppType}");
        cmd.Append($" --db {profile.Database}");

        if (profile.Modules.Count > 0)
            cmd.Append($" --modules {string.Join(",", profile.Modules)}");
        if (profile.Security.Count > 0)
            cmd.Append($" --security {string.Join(",", profile.Security)}");
        if (profile.TenancyMode != "none")
            cmd.Append($" --multitenancy {profile.TenancyMode}");
        if (profile.DevOps.Count > 0)
            cmd.Append($" --devops {string.Join(",", profile.DevOps)}");
        if (profile.AiMode != "none")
            cmd.Append($" --ai {profile.AiMode}");

        return cmd.ToString();
    }

    /// <summary>
    /// Represents all choices from the wizard.
    /// </summary>
    public class SolutionProfile
    {
        public string Name { get; set; } = "MyApp";
        public string Namespace { get; set; } = "MyApp";
        public string OutputDir { get; set; } = "./MyApp";
        public string AppType { get; set; } = "api";
        public string Database { get; set; } = "postgresql";
        public List<string> Modules { get; set; } = new();
        public List<string> Security { get; set; } = new();
        public string TenancyMode { get; set; } = "none";
        public bool IncludeBilling { get; set; }
        public bool IncludeSaaS { get; set; }
        public List<string> Integrations { get; set; } = new();
        public string AiMode { get; set; } = "none";
        public List<string> Observability { get; set; } = new();
        public List<string> DevOps { get; set; } = new();
    }
}
