using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Spectre.Console;
using RVR.CLI.Commands;

// Display welcome banner
AnsiConsole.Write(new FigletText("RVR CLI").Color(Color.Blue));
AnsiConsole.MarkupLine("[grey]RIVORA Framework CLI v4.0.0 - Advanced Edition[/]" + Environment.NewLine);

// Create root command
var rootCommand = new RootCommand("RIVORA Framework CLI - Scaffolding and code generation for .NET applications");

// === NEW COMMAND (interactive wizard + non-interactive flags) ===
var newCommand = new Command("new", "Create a new RIVORA Framework project (interactive wizard)");
var newProjectNameArg = new Argument<string>("name", () => "MyApp", "Name of the project");
var newTemplateOption = new Option<string>("--template", () => "minimal", "Template (minimal, saas-starter, ai-rag, microservices)");
var newTenancyOption = new Option<string>("--tenancy", () => "row", "Multi-tenancy mode (row, schema, database)");
var newTypeOption = new Option<string>("--type", () => "api", "App type (api, api-blazor, microservice, worker)");
var newDbOption = new Option<string>("--db", () => "postgresql", "Database (postgresql, sqlserver, mysql, sqlite, mongodb, cosmosdb, none)");
var newModulesOption = new Option<string>("--modules", () => "", "Comma-separated modules (Caching,HealthChecks,...)");
var newSecurityOption = new Option<string>("--security", () => "", "Comma-separated security options (jwt,apikeys,identity-pro,...)");
var newMultitenancyOption = new Option<string>("--multitenancy", () => "none", "Multi-tenancy mode (none, shared, schema, database)");
var newDevopsOption = new Option<string>("--devops", () => "", "Comma-separated DevOps files (ci,docker-compose,dockerfile,editorconfig,gitignore)");
var newAiOption = new Option<string>("--ai", () => "none", "AI mode (none, ai-base, ai-naturalquery, ai-agents, all)");
var newOutputOption = new Option<string?>("--output", "Output directory");
newCommand.AddArgument(newProjectNameArg);
newCommand.AddOption(newTemplateOption);
newCommand.AddOption(newTenancyOption);
newCommand.AddOption(newTypeOption);
newCommand.AddOption(newDbOption);
newCommand.AddOption(newModulesOption);
newCommand.AddOption(newSecurityOption);
newCommand.AddOption(newMultitenancyOption);
newCommand.AddOption(newDevopsOption);
newCommand.AddOption(newAiOption);
newCommand.AddOption(newOutputOption);
newCommand.SetHandler(async (context) =>
{
    var name = context.ParseResult.GetValueForArgument(newProjectNameArg);
    var template = context.ParseResult.GetValueForOption(newTemplateOption)!;
    var type = context.ParseResult.GetValueForOption(newTypeOption)!;
    var db = context.ParseResult.GetValueForOption(newDbOption)!;
    var modules = context.ParseResult.GetValueForOption(newModulesOption)!;
    var security = context.ParseResult.GetValueForOption(newSecurityOption)!;
    var multitenancy = context.ParseResult.GetValueForOption(newMultitenancyOption)!;
    var devops = context.ParseResult.GetValueForOption(newDevopsOption)!;
    var ai = context.ParseResult.GetValueForOption(newAiOption)!;
    var output = context.ParseResult.GetValueForOption(newOutputOption);

    // If non-default flags provided, use non-interactive mode
    if (!string.IsNullOrEmpty(modules) || !string.IsNullOrEmpty(security) || !string.IsNullOrEmpty(devops) || ai != "none" || multitenancy != "none")
    {
        await NewCommand.ExecuteWithFlagsAsync(name, type, db, modules, security, multitenancy, devops, ai, output);
    }
    else
    {
        await NewCommand.ExecuteAsync(name, template, "row");
    }
});

// === GENERATE COMMAND (with aliases) ===
var generateCommand = new Command("generate", "Generate code (aliases: gen, g)");
generateCommand.AddAlias("gen");
generateCommand.AddAlias("g");

var aggregateCommand = new Command("aggregate", "Generate an aggregate root");
var aggregateNameArg = new Argument<string>("name", "Name of the aggregate");
var aggregateModuleArg = new Argument<string>("module", "Module name");
aggregateCommand.AddArgument(aggregateNameArg);
aggregateCommand.AddArgument(aggregateModuleArg);
aggregateCommand.SetHandler(async (name, module) => await GenerateCommand.GenerateAggregateAsync(name, module), aggregateNameArg, aggregateModuleArg);
generateCommand.AddCommand(aggregateCommand);

var crudCommand = new Command("crud", "Generate CRUD operations");
var crudNameArg = new Argument<string>("name", "Name of the entity");
var crudPropsOption = new Option<string>("--props", () => "", "Properties (Name:type,Name:type)");
crudCommand.AddArgument(crudNameArg);
crudCommand.AddOption(crudPropsOption);
crudCommand.SetHandler(async (name, props) => await GenerateCommand.GenerateCrudAsync(name, props), crudNameArg, crudPropsOption);
generateCommand.AddCommand(crudCommand);

var cmdCommand = new Command("command", "Generate a CQRS command");
var cmdNameArg = new Argument<string>("name", "Name of the command");
cmdCommand.AddArgument(cmdNameArg);
cmdCommand.SetHandler(async (name) => await GenerateCommand.GenerateCommandAsync(name), cmdNameArg);
generateCommand.AddCommand(cmdCommand);

var queryCommand = new Command("query", "Generate a CQRS query");
var queryNameArg = new Argument<string>("name", "Name of the query");
queryCommand.AddArgument(queryNameArg);
queryCommand.SetHandler(async (name) => await GenerateCommand.GenerateQueryAsync(name), queryNameArg);
generateCommand.AddCommand(queryCommand);

var dockerCommand = new Command("docker", "Generate Dockerfile and docker-compose.yml");
var dockerDbOption = new Option<string>("--database", () => "postgresql", "Database provider to use (postgresql, sqlserver)");
dockerCommand.AddOption(dockerDbOption);
dockerCommand.SetHandler(async (database) => await DockerCommand.ExecuteAsync(database), dockerDbOption);
generateCommand.AddCommand(dockerCommand);

// Generate Client
var genClientCommand = new Command("client", "Generate a typed C# HTTP client from an OpenAPI spec");
var genClientUrlOption = new Option<string>("--url", () => "http://localhost:5220", "API base URL");
var genClientOutputOption = new Option<string>("--output", () => "./Generated", "Output directory for generated files");
var genClientNameOption = new Option<string>("--name", () => "RvrApiClient", "Generated client class name");
genClientCommand.AddOption(genClientUrlOption);
genClientCommand.AddOption(genClientOutputOption);
genClientCommand.AddOption(genClientNameOption);
genClientCommand.SetHandler(async (url, output, name) => await GenerateClientCommand.ExecuteAsync(url, output, name), genClientUrlOption, genClientOutputOption, genClientNameOption);
generateCommand.AddCommand(genClientCommand);

// Generate Test
var genTestCommand = new Command("test", "Generate xUnit + FluentAssertions unit tests for an entity");
var genTestEntityArg = new Argument<string>("entity", "Name of the entity to generate tests for");
var genTestOutputOption = new Option<string>("--output", () => "tests", "Output directory for generated test files");
genTestCommand.AddArgument(genTestEntityArg);
genTestCommand.AddOption(genTestOutputOption);
genTestCommand.SetHandler(async (entity, output) => await GenerateTestCommand.ExecuteAsync(entity, output), genTestEntityArg, genTestOutputOption);
generateCommand.AddCommand(genTestCommand);

// === AI COMMAND GROUP ===
var aiCommand = new Command("ai", "AI-powered commands (chat, generate, review)");

// AI Chat
var aiChatCommand = new Command("chat", "Interactive chat with LLM (OpenAI/Claude)");
var aiChatProviderOption = new Option<string>("--provider", () => "openai", "LLM provider (openai, claude)");
var aiChatModelOption = new Option<string>("--model", () => "", "Model to use");
var aiChatApiKeyOption = new Option<string>("--api-key", () => "", "API key (or set environment variable)");
aiChatCommand.AddOption(aiChatProviderOption);
aiChatCommand.AddOption(aiChatModelOption);
aiChatCommand.AddOption(aiChatApiKeyOption);
aiChatCommand.SetHandler(async (provider, model, apiKey) => await AiChatCommand.ExecuteAsync(provider, model, apiKey), aiChatProviderOption, aiChatModelOption, aiChatApiKeyOption);
aiCommand.AddCommand(aiChatCommand);

// AI Generate
var aiGenerateCommand = new Command("generate", "Generate code with AI");
var aiGeneratePromptArg = new Argument<string>("prompt", "Description of code to generate");
var aiGenerateOutputOption = new Option<string?>("--output", "Output file path");
var aiGenerateProviderOption = new Option<string>("--provider", () => "openai", "LLM provider (openai, claude)");
var aiGenerateModelOption = new Option<string>("--model", () => "", "Model to use");
var aiGenerateApiKeyOption = new Option<string>("--api-key", () => "", "API key");
var aiGenerateLanguageOption = new Option<string>("--language", () => "csharp", "Target language");
aiGenerateCommand.AddArgument(aiGeneratePromptArg);
aiGenerateCommand.AddOption(aiGenerateOutputOption);
aiGenerateCommand.AddOption(aiGenerateProviderOption);
aiGenerateCommand.AddOption(aiGenerateModelOption);
aiGenerateCommand.AddOption(aiGenerateApiKeyOption);
aiGenerateCommand.AddOption(aiGenerateLanguageOption);
aiGenerateCommand.SetHandler(async (prompt, output, provider, model, apiKey, language) => await AiGenerateCommand.ExecuteAsync(prompt, output, provider, model, apiKey, language), aiGeneratePromptArg, aiGenerateOutputOption, aiGenerateProviderOption, aiGenerateModelOption, aiGenerateApiKeyOption, aiGenerateLanguageOption);
aiCommand.AddCommand(aiGenerateCommand);

// AI Review
var aiReviewCommand = new Command("review", "AI-powered code review");
var aiReviewPathOption = new Option<string>("--path", () => ".", "Project path");
var aiReviewArchOption = new Option<bool>("--architecture", "Check Clean Architecture conformance");
var aiReviewPerfOption = new Option<bool>("--performance", "Detect performance anti-patterns");
var aiReviewSecOption = new Option<bool>("--security", "Scan for security vulnerabilities");
var aiReviewDddOption = new Option<bool>("--ddd", "Check DDD anti-patterns");
var aiReviewAllOption = new Option<bool>("--all", () => true, "Run all analyzers");
var aiReviewProviderOption = new Option<string?>("--provider", "LLM provider for AI suggestions (openai/claude/ollama)");
var aiReviewApiKeyOption = new Option<string?>("--api-key", "API key for LLM provider");
var aiReviewOutputOption = new Option<string>("--output", () => "console", "Output format (console/json/sarif)");
var aiReviewOutputFileOption = new Option<string?>("--output-file", "Write output to file");
var aiReviewCiOption = new Option<bool>("--ci", "CI mode: non-interactive, exit code reflects findings");
aiReviewCommand.AddOption(aiReviewPathOption);
aiReviewCommand.AddOption(aiReviewArchOption);
aiReviewCommand.AddOption(aiReviewPerfOption);
aiReviewCommand.AddOption(aiReviewSecOption);
aiReviewCommand.AddOption(aiReviewDddOption);
aiReviewCommand.AddOption(aiReviewAllOption);
aiReviewCommand.AddOption(aiReviewProviderOption);
aiReviewCommand.AddOption(aiReviewApiKeyOption);
aiReviewCommand.AddOption(aiReviewOutputOption);
aiReviewCommand.AddOption(aiReviewOutputFileOption);
aiReviewCommand.AddOption(aiReviewCiOption);
aiReviewCommand.SetHandler(async (context) =>
{
    var path = context.ParseResult.GetValueForOption(aiReviewPathOption)!;
    var architecture = context.ParseResult.GetValueForOption(aiReviewArchOption);
    var performance = context.ParseResult.GetValueForOption(aiReviewPerfOption);
    var security = context.ParseResult.GetValueForOption(aiReviewSecOption);
    var ddd = context.ParseResult.GetValueForOption(aiReviewDddOption);
    var all = context.ParseResult.GetValueForOption(aiReviewAllOption);
    var provider = context.ParseResult.GetValueForOption(aiReviewProviderOption);
    var apiKey = context.ParseResult.GetValueForOption(aiReviewApiKeyOption);
    var output = context.ParseResult.GetValueForOption(aiReviewOutputOption)!;
    var outputFile = context.ParseResult.GetValueForOption(aiReviewOutputFileOption);
    var ci = context.ParseResult.GetValueForOption(aiReviewCiOption);

    var exitCode = await AiReviewCommand.ExecuteAsync(
        path, architecture, performance, security, ddd, all,
        provider, apiKey, output, outputFile, ci);

    context.ExitCode = exitCode;
});
aiCommand.AddCommand(aiReviewCommand);

// AI Design
var aiDesignCommand = new Command("design", "AI-assisted domain design");
var aiDesignProviderOption = new Option<string>("--provider", () => "openai", "LLM provider (openai, claude, ollama)");
var aiDesignApiKeyOption = new Option<string?>("--api-key", "API key (or set environment variable)");
var aiDesignModelOption = new Option<string?>("--model", "Model name");
aiDesignCommand.AddOption(aiDesignProviderOption);
aiDesignCommand.AddOption(aiDesignApiKeyOption);
aiDesignCommand.AddOption(aiDesignModelOption);
aiDesignCommand.SetHandler(async (provider, apiKey, model) => await AiDesignCommand.ExecuteAsync(provider, apiKey, model), aiDesignProviderOption, aiDesignApiKeyOption, aiDesignModelOption);
aiCommand.AddCommand(aiDesignCommand);

// === ADD-MODULE COMMAND ===
var addModuleCommand = new Command("add-module", "Add a complete module to the project");
var addModuleNameArg = new Argument<string>("name", "Name of the module");
var addModuleFeatureOption = new Option<string?>("--feature", "Primary feature name");
var addModuleTestsOption = new Option<bool>("--tests", () => true, "Include test project");
var addModuleApiOption = new Option<bool>("--api", () => true, "Include API endpoints");
var addModuleMigrationsOption = new Option<bool>("--migrations", () => true, "Include database migrations");
addModuleCommand.AddArgument(addModuleNameArg);
addModuleCommand.AddOption(addModuleFeatureOption);
addModuleCommand.AddOption(addModuleTestsOption);
addModuleCommand.AddOption(addModuleApiOption);
addModuleCommand.AddOption(addModuleMigrationsOption);
addModuleCommand.SetHandler(async (name, feature, tests, api, migrations) => await AddModuleCommand.ExecuteAsync(name, feature, tests, api, migrations), addModuleNameArg, addModuleFeatureOption, addModuleTestsOption, addModuleApiOption, addModuleMigrationsOption);

// === BENCHMARK COMMAND ===
var benchmarkCommand = new Command("benchmark", "Run load tests with k6");
var benchmarkUrlArg = new Argument<string>("url", "Target URL to test");
var benchmarkDurationOption = new Option<string>("--duration", () => "1m", "Test duration (e.g., 30s, 5m)");
var benchmarkVusOption = new Option<int>("--vus", () => 10, "Number of virtual users");
var benchmarkOutputOption = new Option<string>("--output", () => "console", "Output format (json, html, console)");
var benchmarkScenarioOption = new Option<string>("--scenario", () => "load", "Test scenario (smoke, load, stress, spike, soak)");
benchmarkCommand.AddArgument(benchmarkUrlArg);
benchmarkCommand.AddOption(benchmarkDurationOption);
benchmarkCommand.AddOption(benchmarkVusOption);
benchmarkCommand.AddOption(benchmarkOutputOption);
benchmarkCommand.AddOption(benchmarkScenarioOption);
benchmarkCommand.SetHandler(async (url, duration, vus, output, scenario) => await BenchmarkCommand.ExecuteAsync(url, duration, vus, output, scenario), benchmarkUrlArg, benchmarkDurationOption, benchmarkVusOption, benchmarkOutputOption, benchmarkScenarioOption);

// === DOCTOR COMMAND ===
var doctorCommand = new Command("doctor", "Diagnose project issues");
doctorCommand.SetHandler(async () => await DoctorCommand.ExecuteAsync());

// === DEV COMMAND ===
var devCommand = new Command("dev", "Start development server");
var devWatchOption = new Option<bool>("--watch", () => true, "Enable file watching");
devCommand.AddOption(devWatchOption);
devCommand.SetHandler(async (watch) => await DevCommand.ExecuteAsync(watch), devWatchOption);

// === MIGRATE COMMAND (with subcommands) ===
var migrateCommand = new Command("migrate", "Database migration management");
migrateCommand.SetHandler(async () => await MigrateCommand.ExecuteAsync());

var migrateGenerateCommand = new Command("generate", "Generate a new migration");
var migrateGenNameArg = new Argument<string>("name", "Name of the migration");
migrateGenerateCommand.AddArgument(migrateGenNameArg);
migrateGenerateCommand.SetHandler(async (name) => await MigrateCommand.GenerateAsync(name), migrateGenNameArg);
migrateCommand.AddCommand(migrateGenerateCommand);

var migrateApplyCommand = new Command("apply", "Apply all pending migrations");
migrateApplyCommand.SetHandler(async () => await MigrateCommand.ApplyAsync());
migrateCommand.AddCommand(migrateApplyCommand);

var migrateListCommand = new Command("list", "List all migrations and their status");
migrateListCommand.SetHandler(async () => await MigrateCommand.ListAsync());
migrateCommand.AddCommand(migrateListCommand);

var migrateRollbackCommand = new Command("rollback", "Rollback to the previous migration");
migrateRollbackCommand.SetHandler(async () => await MigrateCommand.RollbackAsync());
migrateCommand.AddCommand(migrateRollbackCommand);

// === REMOVE-MODULE COMMAND ===
var removeModuleCommand = new Command("remove-module", "Remove a module from the project (inverse of add-module)");
var removeModuleNameArg = new Argument<string>("name", "Name of the module to remove");
var removeModuleDryRunOption = new Option<bool>("--dry-run", "Preview changes without applying them");
var removeModuleForceOption = new Option<bool>("--force", "Ignore dependency warnings");
removeModuleCommand.AddArgument(removeModuleNameArg);
removeModuleCommand.AddOption(removeModuleDryRunOption);
removeModuleCommand.AddOption(removeModuleForceOption);
removeModuleCommand.SetHandler(async (name, dryRun, force) => await RemoveModuleCommand.ExecuteAsync(name, dryRun, force), removeModuleNameArg, removeModuleDryRunOption, removeModuleForceOption);

// === SEED COMMAND (with options) ===
var seedCommand = new Command("seed", "Seed database with test/demo data");
var seedProfileOption = new Option<string>("--profile", () => "dev", "Seeding profile (dev, demo, test, perf)");
var seedResetOption = new Option<bool>("--reset", "Truncate database before seeding");
var seedDryRunOption = new Option<bool>("--dry-run", "Show what would be seeded without executing");
var seedTenantOption = new Option<string?>("--tenant", "Seed a specific tenant (multi-tenant support)");
seedCommand.AddOption(seedProfileOption);
seedCommand.AddOption(seedResetOption);
seedCommand.AddOption(seedDryRunOption);
seedCommand.AddOption(seedTenantOption);
seedCommand.SetHandler(async (profile, reset, dryRun, tenant) => await SeedCommand.ExecuteAsync(profile, reset, dryRun, tenant), seedProfileOption, seedResetOption, seedDryRunOption, seedTenantOption);

// Generate Seed (subcommand under generate)
var genSeedCommand = new Command("seed", "Generate a data seeder scaffold for an entity");
var genSeedEntityArg = new Argument<string>("entity", "Name of the entity to generate a seeder for");
var genSeedProfileOption = new Option<string>("--profile", () => "dev", "Default seeding profile");
genSeedCommand.AddArgument(genSeedEntityArg);
genSeedCommand.AddOption(genSeedProfileOption);
genSeedCommand.SetHandler(async (entity, profile) => await SeedCommand.GenerateAsync(entity, profile), genSeedEntityArg, genSeedProfileOption);
generateCommand.AddCommand(genSeedCommand);

// === PUBLISH COMMAND ===
var publishCommand = new Command("publish", "Publish application (Docker, NuGet, Azure, self-contained)");
var publishTargetOption = new Option<string>("--target", () => "auto", "Publish target (docker, nuget, azure, self-contained, auto)");
var publishSkipTestsOption = new Option<bool>("--skip-tests", "Skip running tests before publishing");
var publishDryRunOption = new Option<bool>("--dry-run", "Show commands without executing them");
var publishRegistryOption = new Option<string?>("--registry", "Container/package registry URL");
var publishTagOption = new Option<string?>("--tag", "Version tag override");
publishCommand.AddOption(publishTargetOption);
publishCommand.AddOption(publishSkipTestsOption);
publishCommand.AddOption(publishDryRunOption);
publishCommand.AddOption(publishRegistryOption);
publishCommand.AddOption(publishTagOption);
publishCommand.SetHandler(async (target, skipTests, dryRun, registry, tag) => await PublishCommand.ExecuteAsync(target, skipTests, dryRun, registry, tag), publishTargetOption, publishSkipTestsOption, publishDryRunOption, publishRegistryOption, publishTagOption);

// === ENV COMMAND (with subcommands) ===
var envCommand = new Command("env", "Manage environments and secrets");

var envListCommand = new Command("list", "List available environments");
envListCommand.SetHandler(async () => await EnvCommand.ListAsync());
envCommand.AddCommand(envListCommand);

var envGetCommand = new Command("get", "Get a configuration value");
var envGetKeyArg = new Argument<string>("key", "Configuration key (supports nested keys with ':' separator)");
envGetCommand.AddArgument(envGetKeyArg);
envGetCommand.SetHandler(async (key) => await EnvCommand.GetAsync(key), envGetKeyArg);
envCommand.AddCommand(envGetCommand);

var envSetCommand = new Command("set", "Set a configuration value");
var envSetKeyArg = new Argument<string>("key", "Configuration key");
var envSetValueArg = new Argument<string>("value", "Configuration value");
envSetCommand.AddArgument(envSetKeyArg);
envSetCommand.AddArgument(envSetValueArg);
envSetCommand.SetHandler(async (key, value) => await EnvCommand.SetAsync(key, value), envSetKeyArg, envSetValueArg);
envCommand.AddCommand(envSetCommand);

var envRemoveCommand = new Command("remove", "Remove a configuration key");
var envRemoveKeyArg = new Argument<string>("key", "Configuration key to remove");
envRemoveCommand.AddArgument(envRemoveKeyArg);
envRemoveCommand.SetHandler(async (key) => await EnvCommand.RemoveAsync(key), envRemoveKeyArg);
envCommand.AddCommand(envRemoveCommand);

var envSwitchCommand = new Command("switch", "Switch active environment");
var envSwitchEnvArg = new Argument<string>("environment", "Target environment (Development, Staging, Production)");
envSwitchCommand.AddArgument(envSwitchEnvArg);
envSwitchCommand.SetHandler(async (env) => await EnvCommand.SwitchAsync(env), envSwitchEnvArg);
envCommand.AddCommand(envSwitchCommand);

var envDiffCommand = new Command("diff", "Compare two environments");
var envDiffEnv1Arg = new Argument<string>("env1", "First environment");
var envDiffEnv2Arg = new Argument<string>("env2", "Second environment");
envDiffCommand.AddArgument(envDiffEnv1Arg);
envDiffCommand.AddArgument(envDiffEnv2Arg);
envDiffCommand.SetHandler(async (env1, env2) => await EnvCommand.DiffAsync(env1, env2), envDiffEnv1Arg, envDiffEnv2Arg);
envCommand.AddCommand(envDiffCommand);

var envSecretsCommand = new Command("secrets", "Manage .NET User Secrets");

var envSecretsInitCommand = new Command("init", "Initialize .NET User Secrets");
envSecretsInitCommand.SetHandler(async () => await EnvCommand.SecretsInitAsync());
envSecretsCommand.AddCommand(envSecretsInitCommand);

var envSecretsSetCommand = new Command("set", "Set a secret value");
var envSecretsSetKeyArg = new Argument<string>("key", "Secret key");
var envSecretsSetValueArg = new Argument<string>("value", "Secret value");
envSecretsSetCommand.AddArgument(envSecretsSetKeyArg);
envSecretsSetCommand.AddArgument(envSecretsSetValueArg);
envSecretsSetCommand.SetHandler(async (key, value) => await EnvCommand.SecretsSetAsync(key, value), envSecretsSetKeyArg, envSecretsSetValueArg);
envSecretsCommand.AddCommand(envSecretsSetCommand);

envCommand.AddCommand(envSecretsCommand);

var envExportCommand = new Command("export", "Export configuration to file");
var envExportFormatOption = new Option<string>("--format", () => "dotenv", "Export format (dotenv, json, yaml)");
envExportCommand.AddOption(envExportFormatOption);
envExportCommand.SetHandler(async (format) => await EnvCommand.ExportAsync(format), envExportFormatOption);
envCommand.AddCommand(envExportCommand);

var envImportCommand = new Command("import", "Import configuration from .env file");
var envImportFileOption = new Option<string>("--file", () => ".env", "File to import");
envImportCommand.AddOption(envImportFileOption);
envImportCommand.SetHandler(async (file) => await EnvCommand.ImportAsync(file), envImportFileOption);
envCommand.AddCommand(envImportCommand);

// === UPGRADE COMMAND ===
var upgradeCommand = new Command("upgrade", "Migration assistant between major RIVORA versions");
var upgradeTargetOption = new Option<string?>("--to", "Target version to upgrade to");
var upgradeDryRunOption = new Option<bool>("--dry-run", "Preview changes without applying them");
var upgradeListOption = new Option<bool>("--list", "List available migrations");
upgradeCommand.AddOption(upgradeTargetOption);
upgradeCommand.AddOption(upgradeDryRunOption);
upgradeCommand.AddOption(upgradeListOption);
upgradeCommand.SetHandler(async (target, dryRun, list) => await UpgradeCommand.ExecuteAsync(target, dryRun, list), upgradeTargetOption, upgradeDryRunOption, upgradeListOption);

// === COMPLETION COMMAND ===
var completionCommand = new Command("completion", "Generate shell completion script");
var completionShellArg = new Argument<string>("shell", "Target shell (bash, zsh, pwsh)");
completionCommand.AddArgument(completionShellArg);
completionCommand.SetHandler(async (shell) => await CompletionCommand.ExecuteAsync(shell), completionShellArg);

// Add all commands to root
rootCommand.AddCommand(newCommand);
rootCommand.AddCommand(generateCommand);
rootCommand.AddCommand(aiCommand);
rootCommand.AddCommand(addModuleCommand);
rootCommand.AddCommand(removeModuleCommand);
rootCommand.AddCommand(benchmarkCommand);
rootCommand.AddCommand(doctorCommand);
rootCommand.AddCommand(devCommand);
rootCommand.AddCommand(migrateCommand);
rootCommand.AddCommand(seedCommand);
rootCommand.AddCommand(publishCommand);
rootCommand.AddCommand(envCommand);
rootCommand.AddCommand(upgradeCommand);
rootCommand.AddCommand(completionCommand);

// Build and run parser
var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .Build();

await parser.InvokeAsync(args);
