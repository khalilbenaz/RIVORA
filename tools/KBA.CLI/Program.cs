using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Spectre.Console;
using KBA.CLI.Commands;

// Display welcome banner
AnsiConsole.Write(new FigletText("KBA CLI").Color(Color.Blue));
AnsiConsole.MarkupLine("[grey]KBA Framework CLI v2.0.0 - Wave 5 Edition[/]" + Environment.NewLine);

// Create root command
var rootCommand = new RootCommand("KBA Framework CLI - Scaffolding and code generation for .NET applications");

// === NEW COMMAND ===
var newCommand = new Command("new", "Create a new KBA Framework project");
var newProjectNameArg = new Argument<string>("name", "Name of the project");
var newTemplateOption = new Option<string>("--template", () => "minimal", "Template to use (minimal, saas-starter, ai-rag)");
var newTenancyOption = new Option<string>("--tenancy", () => "row", "Multi-tenancy mode (row, schema, database)");
newCommand.AddArgument(newProjectNameArg);
newCommand.AddOption(newTemplateOption);
newCommand.AddOption(newTenancyOption);
newCommand.SetHandler(async (name, template, tenancy) => await NewCommand.ExecuteAsync(name, template, tenancy), newProjectNameArg, newTemplateOption, newTenancyOption);

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
var aiReviewCommand = new Command("review", "Review code with AI");
var aiReviewPathArg = new Argument<string>("path", "File or directory to review");
var aiReviewProviderOption = new Option<string>("--provider", () => "openai", "LLM provider (openai, claude)");
var aiReviewModelOption = new Option<string>("--model", () => "", "Model to use");
var aiReviewApiKeyOption = new Option<string>("--api-key", () => "", "API key");
var aiReviewFocusOption = new Option<string>("--focus", () => "all", "Focus area (security, performance, style, all)");
aiReviewCommand.AddArgument(aiReviewPathArg);
aiReviewCommand.AddOption(aiReviewProviderOption);
aiReviewCommand.AddOption(aiReviewModelOption);
aiReviewCommand.AddOption(aiReviewApiKeyOption);
aiReviewCommand.AddOption(aiReviewFocusOption);
aiReviewCommand.SetHandler(async (path, provider, model, apiKey, focus) => await AiReviewCommand.ExecuteAsync(path, provider, model, apiKey, focus), aiReviewPathArg, aiReviewProviderOption, aiReviewModelOption, aiReviewApiKeyOption, aiReviewFocusOption);
aiCommand.AddCommand(aiReviewCommand);

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

// === MIGRATE COMMAND ===
var migrateCommand = new Command("migrate", "Apply database migrations");
migrateCommand.SetHandler(async () => await MigrateCommand.ExecuteAsync());

// === SEED COMMAND ===
var seedCommand = new Command("seed", "Seed database with demo data");
seedCommand.SetHandler(async () => await SeedCommand.ExecuteAsync());

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
rootCommand.AddCommand(benchmarkCommand);
rootCommand.AddCommand(doctorCommand);
rootCommand.AddCommand(devCommand);
rootCommand.AddCommand(migrateCommand);
rootCommand.AddCommand(seedCommand);
rootCommand.AddCommand(completionCommand);

// Build and run parser
var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .Build();

await parser.InvokeAsync(args);
