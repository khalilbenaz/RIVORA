using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Spectre.Console;
using KBA.CLI.Commands;

AnsiConsole.Write(new FigletText("KBA CLI").Color(Color.Blue));
AnsiConsole.MarkupLine("[grey]KBA Framework CLI v2.0.0[/]" + Environment.NewLine);

var rootCommand = new RootCommand("KBA Framework CLI - Scaffolding and code generation for .NET applications");

var newCommand = new Command("new", "Create a new KBA Framework project");
var newProjectNameArg = new Argument<string>("name", "Name of the project");
var newTemplateOption = new Option<string>("--template", () => "minimal", "Template to use (minimal, saas-starter, ai-rag)");
var newTenancyOption = new Option<string>("--tenancy", () => "row", "Multi-tenancy mode (row, schema, database)");
newCommand.AddArgument(newProjectNameArg);
newCommand.AddOption(newTemplateOption);
newCommand.AddOption(newTenancyOption);
newCommand.SetHandler(async (name, template, tenancy) => await NewCommand.ExecuteAsync(name, template, tenancy), newProjectNameArg, newTemplateOption, newTenancyOption);

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

var devCommand = new Command("dev", "Start development server");
var devWatchOption = new Option<bool>("--watch", () => true, "Enable file watching");
devCommand.AddOption(devWatchOption);
devCommand.SetHandler(async (watch) => await DevCommand.ExecuteAsync(watch), devWatchOption);

var migrateCommand = new Command("migrate", "Apply database migrations");
migrateCommand.SetHandler(async () => await MigrateCommand.ExecuteAsync());

var seedCommand = new Command("seed", "Seed database with demo data");
seedCommand.SetHandler(async () => await SeedCommand.ExecuteAsync());

var doctorCommand = new Command("doctor", "Diagnose project issues");
doctorCommand.SetHandler(async () => await DoctorCommand.ExecuteAsync());

rootCommand.AddCommand(newCommand);
rootCommand.AddCommand(generateCommand);
rootCommand.AddCommand(devCommand);
rootCommand.AddCommand(migrateCommand);
rootCommand.AddCommand(seedCommand);
rootCommand.AddCommand(doctorCommand);

var parser = new CommandLineBuilder(rootCommand).UseDefaults().Build();
await parser.InvokeAsync(args);
