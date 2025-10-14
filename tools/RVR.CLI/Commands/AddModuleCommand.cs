using Spectre.Console;
using System.Text;

namespace RVR.CLI.Commands;

/// <summary>
/// Provides functionality to add complete modules to a RIVORA Framework project.
/// </summary>
public static class AddModuleCommand
{
    /// <summary>
    /// Executes the add-module command to create a complete module structure.
    /// </summary>
    /// <param name="moduleName">Name of the module to create.</param>
    /// <param name="featureName">Primary feature name (optional).</param>
    /// <param name="includeTests">Include test project.</param>
    /// <param name="includeApi">Include API endpoints.</param>
    /// <param name="includeMigrations">Include database migrations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(
        string moduleName,
        string? featureName,
        bool includeTests,
        bool includeApi,
        bool includeMigrations)
    {
        AnsiConsole.Write(new FigletText("Add Module").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]Creating complete module structure[/]" + Environment.NewLine);

        var config = new
        {
            ModuleName = moduleName,
            FeatureName = featureName ?? moduleName,
            IncludeTests = includeTests,
            IncludeApi = includeApi,
            IncludeMigrations = includeMigrations
        };

        var configTable = new Table();
        configTable.AddColumn("Option");
        configTable.AddColumn("Value");
        configTable.AddRow("[cyan]Module Name[/]", config.ModuleName);
        configTable.AddRow("[cyan]Feature Name[/]", config.FeatureName);
        configTable.AddRow("[cyan]Include Tests[/]", config.IncludeTests ? "Yes" : "No");
        configTable.AddRow("[cyan]Include API[/]", config.IncludeApi ? "Yes" : "No");
        configTable.AddRow("[cyan]Include Migrations[/]", config.IncludeMigrations ? "Yes" : "No");
        AnsiConsole.Write(configTable);
        AnsiConsole.WriteLine();

        var createdFiles = new List<string>();

        // Create module structure
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
                // Domain layer
                var domainTask = ctx.AddTask("[cyan]Creating Domain layer...[/]", maxValue: 100);
                createdFiles.AddRange(CreateDomainLayer(moduleName, config.FeatureName));
                domainTask.Increment(100);

                // Application layer
                var appTask = ctx.AddTask("[cyan]Creating Application layer...[/]", maxValue: 100);
                createdFiles.AddRange(CreateApplicationLayer(moduleName, config.FeatureName));
                appTask.Increment(100);

                // Infrastructure layer
                var infraTask = ctx.AddTask("[cyan]Creating Infrastructure layer...[/]", maxValue: 100);
                createdFiles.AddRange(CreateInfrastructureLayer(moduleName, config.IncludeMigrations));
                infraTask.Increment(100);

                // API layer (optional)
                if (config.IncludeApi)
                {
                    var apiTask = ctx.AddTask("[cyan]Creating API layer...[/]", maxValue: 100);
                    createdFiles.AddRange(CreateApiLayer(moduleName));
                    apiTask.Increment(100);
                }

                // Tests (optional)
                if (config.IncludeTests)
                {
                    var testTask = ctx.AddTask("[cyan]Creating Test project...[/]", maxValue: 100);
                    createdFiles.AddRange(CreateTestProject(moduleName));
                    testTask.Increment(100);
                }
            });

        // Display results
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]✓ Module '{moduleName}' created successfully![/]");
        AnsiConsole.WriteLine();

        var filesTable = new Table();
        filesTable.AddColumn("Created Files");
        foreach (var file in createdFiles.OrderBy(f => f))
        {
            filesTable.AddRow($"[green]{file}[/]");
        }
        AnsiConsole.Write(filesTable);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Next steps:[/]");
        AnsiConsole.MarkupLine($"  1. Review and customize the generated files in [cyan]src/RVR.Framework.{moduleName}.*[/]");
        if (config.IncludeApi)
            AnsiConsole.MarkupLine($"  2. Register API endpoints in [cyan]Program.cs[/]");
        if (config.IncludeMigrations)
            AnsiConsole.MarkupLine($"  3. Run [cyan]dotnet ef database update[/] to apply migrations");
        AnsiConsole.MarkupLine($"  4. Run [cyan]rvr dev[/] to start the development server");

        await Task.CompletedTask;
    }

    private static IEnumerable<string> CreateDomainLayer(string moduleName, string featureName)
    {
        var files = new List<string>();
        var domainDir = $"src/RVR.Framework.{moduleName}.Domain";
        var entitiesDir = Path.Combine(domainDir, "Entities");
        var eventsDir = Path.Combine(domainDir, "Events");
        var exceptionsDir = Path.Combine(domainDir, "Exceptions");

        Directory.CreateDirectory(entitiesDir);
        Directory.CreateDirectory(eventsDir);
        Directory.CreateDirectory(exceptionsDir);

        // Entity
        var entityFile = Path.Combine(entitiesDir, $"{featureName}.cs");
        File.WriteAllText(entityFile, GenerateEntityCode(moduleName, featureName));
        files.Add(entityFile);

        // Domain events
        var createdEventFile = Path.Combine(eventsDir, $"{featureName}CreatedEvent.cs");
        File.WriteAllText(createdEventFile, GenerateDomainEventCode(featureName, "Created"));
        files.Add(createdEventFile);

        var updatedEventFile = Path.Combine(eventsDir, $"{featureName}UpdatedEvent.cs");
        File.WriteAllText(updatedEventFile, GenerateDomainEventCode(featureName, "Updated"));
        files.Add(updatedEventFile);

        // Exception
        var exceptionFile = Path.Combine(exceptionsDir, $"{featureName}NotFoundException.cs");
        File.WriteAllText(exceptionFile, GenerateNotFoundExceptionCode(featureName));
        files.Add(exceptionFile);

        // Domain project file
        var domainCsproj = Path.Combine(domainDir, $"RVR.Framework.{moduleName}.Domain.csproj");
        File.WriteAllText(domainCsproj, GenerateDomainCsproj(moduleName));
        files.Add(domainCsproj);

        return files;
    }

    private static IEnumerable<string> CreateApplicationLayer(string moduleName, string featureName)
    {
        var files = new List<string>();
        var appDir = $"src/RVR.Framework.{moduleName}.Application";
        var featuresDir = Path.Combine(appDir, "Features", featureName);
        var commonDir = Path.Combine(appDir, "Common");

        Directory.CreateDirectory(featuresDir);
        Directory.CreateDirectory(commonDir);

        // Create command
        var createCommandDir = Path.Combine(featuresDir, "Create");
        Directory.CreateDirectory(createCommandDir);
        var createCommandFile = Path.Combine(createCommandDir, $"Create{featureName}Command.cs");
        File.WriteAllText(createCommandFile, GenerateCreateCommandCode(moduleName, featureName));
        files.Add(createCommandFile);

        // Update command
        var updateCommandDir = Path.Combine(featuresDir, "Update");
        Directory.CreateDirectory(updateCommandDir);
        var updateCommandFile = Path.Combine(updateCommandDir, $"Update{featureName}Command.cs");
        File.WriteAllText(updateCommandFile, GenerateUpdateCommandCode(moduleName, featureName));
        files.Add(updateCommandFile);

        // Get by ID query
        var getQueryDir = Path.Combine(featuresDir, "GetById");
        Directory.CreateDirectory(getQueryDir);
        var getQueryFile = Path.Combine(getQueryDir, $"Get{featureName}ByIdQuery.cs");
        File.WriteAllText(getQueryFile, GenerateGetByIdQueryCode(moduleName, featureName));
        files.Add(getQueryFile);

        // Get all query
        var getAllQueryDir = Path.Combine(featuresDir, "GetAll");
        Directory.CreateDirectory(getAllQueryDir);
        var getAllQueryFile = Path.Combine(getAllQueryDir, $"GetAll{featureName}Query.cs");
        File.WriteAllText(getAllQueryFile, GenerateGetAllQueryCode(moduleName, featureName));
        files.Add(getAllQueryFile);

        // Delete command
        var deleteCommandDir = Path.Combine(featuresDir, "Delete");
        Directory.CreateDirectory(deleteCommandDir);
        var deleteCommandFile = Path.Combine(deleteCommandDir, $"Delete{featureName}Command.cs");
        File.WriteAllText(deleteCommandFile, GenerateDeleteCommandCode(moduleName, featureName));
        files.Add(deleteCommandFile);

        // DTOs
        var dtoFile = Path.Combine(commonDir, $"{featureName}Dto.cs");
        File.WriteAllText(dtoFile, GenerateDtoCode(featureName));
        files.Add(dtoFile);

        // Application project file
        var appCsproj = Path.Combine(appDir, $"RVR.Framework.{moduleName}.Application.csproj");
        File.WriteAllText(appCsproj, GenerateApplicationCsproj(moduleName));
        files.Add(appCsproj);

        return files;
    }

    private static IEnumerable<string> CreateInfrastructureLayer(string moduleName, bool includeMigrations)
    {
        var files = new List<string>();
        var infraDir = $"src/RVR.Framework.{moduleName}.Infrastructure";
        var reposDir = Path.Combine(infraDir, "Repositories");
        var configDir = Path.Combine(infraDir, "Configurations");
        var migrationsDir = Path.Combine(infraDir, "Migrations");

        Directory.CreateDirectory(reposDir);
        Directory.CreateDirectory(configDir);
        if (includeMigrations)
            Directory.CreateDirectory(migrationsDir);

        // Repository
        var repoFile = Path.Combine(reposDir, $"{moduleName}Repository.cs");
        File.WriteAllText(repoFile, GenerateRepositoryCode(moduleName));
        files.Add(repoFile);

        // Repository interface
        var repoInterfaceFile = Path.Combine(reposDir, $"I{moduleName}Repository.cs");
        File.WriteAllText(repoInterfaceFile, GenerateRepositoryInterfaceCode(moduleName));
        files.Add(repoInterfaceFile);

        // Entity configuration
        var configFileName = Path.Combine(configDir, $"{moduleName}Configuration.cs");
        File.WriteAllText(configFileName, GenerateEntityConfigurationCode(moduleName));
        files.Add(configFileName);

        // Infrastructure project file
        var infraCsproj = Path.Combine(infraDir, $"RVR.Framework.{moduleName}.Infrastructure.csproj");
        File.WriteAllText(infraCsproj, GenerateInfrastructureCsproj(moduleName, includeMigrations));
        files.Add(infraCsproj);

        return files;
    }

    private static IEnumerable<string> CreateApiLayer(string moduleName)
    {
        var files = new List<string>();
        var apiDir = $"src/RVR.Framework.{moduleName}.Api";
        var controllersDir = Path.Combine(apiDir, "Controllers");

        Directory.CreateDirectory(controllersDir);

        // Controller
        var controllerFile = Path.Combine(controllersDir, $"{moduleName}Controller.cs");
        File.WriteAllText(controllerFile, GenerateControllerCode(moduleName));
        files.Add(controllerFile);

        // API project file
        var apiCsproj = Path.Combine(apiDir, $"RVR.Framework.{moduleName}.Api.csproj");
        File.WriteAllText(apiCsproj, GenerateApiCsproj(moduleName));
        files.Add(apiCsproj);

        return files;
    }

    private static IEnumerable<string> CreateTestProject(string moduleName)
    {
        var files = new List<string>();
        var testDir = $"tests/RVR.Framework.{moduleName}.Tests";
        var domainTestsDir = Path.Combine(testDir, "Domain");
        var applicationTestsDir = Path.Combine(testDir, "Application");

        Directory.CreateDirectory(domainTestsDir);
        Directory.CreateDirectory(applicationTestsDir);

        // Domain tests
        var domainTestFile = Path.Combine(domainTestsDir, $"{moduleName}Tests.cs");
        File.WriteAllText(domainTestFile, GenerateDomainTestsCode(moduleName));
        files.Add(domainTestFile);

        // Application tests
        var appTestFile = Path.Combine(applicationTestsDir, $"{moduleName}HandlerTests.cs");
        File.WriteAllText(appTestFile, GenerateApplicationTestsCode(moduleName));
        files.Add(appTestFile);

        // Test project file
        var testCsproj = Path.Combine(testDir, $"RVR.Framework.{moduleName}.Tests.csproj");
        File.WriteAllText(testCsproj, GenerateTestCsproj(moduleName));
        files.Add(testCsproj);

        return files;
    }

    // Code generation templates
    private static string GenerateEntityCode(string moduleName, string featureName) => $@"using RVR.Framework.Core.Entities;

namespace RVR.Framework.{moduleName}.Domain.Entities;

/// <summary>
/// Represents a {featureName} entity.
/// </summary>
public class {featureName} : Entity<Guid>, IAuditable
{{
    public string Name {{ get; private set; }} = string.Empty;
    public string Description {{ get; private set; }} = string.Empty;
    public bool IsActive {{ get; private set; }} = true;
    
    public DateTime CreatedAt {{ get; set; }}
    public Guid? CreatedBy {{ get; set; }}
    public DateTime UpdatedAt {{ get; set; }}
    public Guid? UpdatedBy {{ get; set; }}

    private {featureName}() {{ }}

    public static {featureName} Create(string name, string description)
    {{
        var entity = new {featureName}
        {{
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }};
        
        entity.AddEvent(new {featureName}CreatedEvent(entity.Id));
        return entity;
    }}

    public void Update(string name, string description)
    {{
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new {featureName}UpdatedEvent(Id));
    }}
}}";

    private static string GenerateDomainEventCode(string featureName, string eventType) => $@"using RVR.Framework.Core.Events;

namespace RVR.Framework.{featureName}.Domain.Events;

/// <summary>
/// Domain event raised when a {featureName} is {eventType.ToLower()}.
/// </summary>
public record {featureName}{eventType}Event(Guid Id) : DomainEvent;";

    private static string GenerateNotFoundExceptionCode(string featureName) => $@"namespace RVR.Framework.{featureName}.Domain.Exceptions;

/// <summary>
/// Exception thrown when a {featureName} is not found.
/// </summary>
public class {featureName}NotFoundException : NotFoundException
{{
    public {featureName}NotFoundException(Guid id) 
        : base($""{featureName} with ID '{{$id}}' not found."") {{ }}
}}";

    private static string GenerateDomainCsproj(string moduleName) => $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>RVR.Framework.{moduleName}.Domain</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\..\\src\\RVR.Framework.Core\\RVR.Framework.Core.csproj"" />
  </ItemGroup>

</Project>";

    private static string GenerateCreateCommandCode(string moduleName, string featureName) => $@"using MediatR;
using RVR.Framework.{moduleName}.Domain.Entities;
using RVR.Framework.{moduleName}.Domain.Repositories;

namespace RVR.Framework.{moduleName}.Application.Features.{featureName}.Create;

/// <summary>
/// Command to create a new {featureName}.
/// </summary>
public record Create{featureName}Command(string Name, string Description) : IRequest<{featureName}Dto>;

/// <summary>
/// Handler for Create{featureName}Command.
/// </summary>
public class Create{featureName}CommandHandler : IRequestHandler<Create{featureName}Command, {featureName}Dto>
{{
    private readonly I{moduleName}Repository _repository;

    public Create{featureName}CommandHandler(I{moduleName}Repository repository)
    {{
        _repository = repository;
    }}

    public async Task<{featureName}Dto> Handle(Create{featureName}Command request, CancellationToken cancellationToken)
    {{
        var entity = {featureName}.Create(request.Name, request.Description);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return {featureName}Dto.FromEntity(entity);
    }}
}}";

    private static string GenerateUpdateCommandCode(string moduleName, string featureName) => $@"using MediatR;
using RVR.Framework.{moduleName}.Domain.Repositories;
using RVR.Framework.{moduleName}.Domain.Exceptions;

namespace RVR.Framework.{moduleName}.Application.Features.{featureName}.Update;

/// <summary>
/// Command to update an existing {featureName}.
/// </summary>
public record Update{featureName}Command(Guid Id, string Name, string Description) : IRequest<{featureName}Dto>;

/// <summary>
/// Handler for Update{featureName}Command.
/// </summary>
public class Update{featureName}CommandHandler : IRequestHandler<Update{featureName}Command, {featureName}Dto>
{{
    private readonly I{moduleName}Repository _repository;

    public Update{featureName}CommandHandler(I{moduleName}Repository repository)
    {{
        _repository = repository;
    }}

    public async Task<{featureName}Dto> Handle(Update{featureName}Command request, CancellationToken cancellationToken)
    {{
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new {featureName}NotFoundException(request.Id);

        entity.Update(request.Name, request.Description);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return {featureName}Dto.FromEntity(entity);
    }}
}}";

    private static string GenerateGetByIdQueryCode(string moduleName, string featureName) => $@"using MediatR;
using RVR.Framework.{moduleName}.Domain.Repositories;
using RVR.Framework.{moduleName}.Domain.Exceptions;

namespace RVR.Framework.{moduleName}.Application.Features.{featureName}.GetById;

/// <summary>
/// Query to get a {featureName} by ID.
/// </summary>
public record Get{featureName}ByIdQuery(Guid Id) : IRequest<{featureName}Dto>;

/// <summary>
/// Handler for Get{featureName}ByIdQuery.
/// </summary>
public class Get{featureName}ByIdQueryHandler : IRequestHandler<Get{featureName}ByIdQuery, {featureName}Dto>
{{
    private readonly I{moduleName}Repository _repository;

    public Get{featureName}ByIdQueryHandler(I{moduleName}Repository repository)
    {{
        _repository = repository;
    }}

    public async Task<{featureName}Dto> Handle(Get{featureName}ByIdQuery request, CancellationToken cancellationToken)
    {{
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new {featureName}NotFoundException(request.Id);

        return {featureName}Dto.FromEntity(entity);
    }}
}}";

    private static string GenerateGetAllQueryCode(string moduleName, string featureName) => $@"using MediatR;
using RVR.Framework.{moduleName}.Domain.Repositories;

namespace RVR.Framework.{moduleName}.Application.Features.{featureName}.GetAll;

/// <summary>
/// Query to get all {featureName} entities.
/// </summary>
public record GetAll{featureName}Query() : IRequest<IEnumerable<{featureName}Dto>>;

/// <summary>
/// Handler for GetAll{featureName}Query.
/// </summary>
public class GetAll{featureName}QueryHandler : IRequestHandler<GetAll{featureName}Query, IEnumerable<{featureName}Dto>>
{{
    private readonly I{moduleName}Repository _repository;

    public GetAll{featureName}QueryHandler(I{moduleName}Repository repository)
    {{
        _repository = repository;
    }}

    public async Task<IEnumerable<{featureName}Dto>> Handle(GetAll{featureName}Query request, CancellationToken cancellationToken)
    {{
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select({featureName}Dto.FromEntity);
    }}
}}";

    private static string GenerateDeleteCommandCode(string moduleName, string featureName) => $@"using MediatR;
using RVR.Framework.{moduleName}.Domain.Repositories;

namespace RVR.Framework.{moduleName}.Application.Features.{featureName}.Delete;

/// <summary>
/// Command to delete a {featureName}.
/// </summary>
public record Delete{featureName}Command(Guid Id) : IRequest<Unit>;

/// <summary>
/// Handler for Delete{featureName}Command.
/// </summary>
public class Delete{featureName}CommandHandler : IRequestHandler<Delete{featureName}Command, Unit>
{{
    private readonly I{moduleName}Repository _repository;

    public Delete{featureName}CommandHandler(I{moduleName}Repository repository)
    {{
        _repository = repository;
    }}

    public async Task<Unit> Handle(Delete{featureName}Command request, CancellationToken cancellationToken)
    {{
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity != null)
        {{
            _repository.Delete(entity);
            await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }}
        return Unit.Value;
    }}
}}";

    private static string GenerateDtoCode(string featureName) => $@"using RVR.Framework.{featureName}.Domain.Entities;

namespace RVR.Framework.{featureName}.Application.Common;

/// <summary>
/// DTO for {featureName} entity.
/// </summary>
public record {featureName}Dto
{{
    public Guid Id {{ get; init; }}
    public string Name {{ get; init; }} = string.Empty;
    public string Description {{ get; init; }} = string.Empty;
    public bool IsActive {{ get; init; }}
    public DateTime CreatedAt {{ get; init; }}
    public DateTime UpdatedAt {{ get; init; }}

    public static {featureName}Dto FromEntity({featureName} entity) => new()
    {{
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    }};
}}";

    private static string GenerateApplicationCsproj(string moduleName) => $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>RVR.Framework.{moduleName}.Application</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""MediatR"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\..\\src\\RVR.Framework.Core\\RVR.Framework.Core.csproj"" />
    <ProjectReference Include=""..\\RVR.Framework.{moduleName}.Domain\\RVR.Framework.{moduleName}.Domain.csproj"" />
  </ItemGroup>

</Project>";

    private static string GenerateRepositoryCode(string moduleName) => $@"using RVR.Framework.Data;
using RVR.Framework.{moduleName}.Domain.Entities;
using RVR.Framework.{moduleName}.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.{moduleName}.Infrastructure.Repositories;

/// <summary>
/// Entity Framework repository for {moduleName}.
/// </summary>
public class {moduleName}Repository : I{moduleName}Repository
{{
    private readonly AppDbContext _context;

    public {moduleName}Repository(AppDbContext context)
    {{
        _context = context;
    }}

    public IUnitOfWork UnitOfWork => _context;

    public async Task<{moduleName}?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {{
        return await _context.Set<{moduleName}>().FindAsync(new object[] {{ id }}, ct);
    }}

    public async Task<IEnumerable<{moduleName}>> GetAllAsync(CancellationToken ct = default)
    {{
        return await _context.Set<{moduleName}>()
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
    }}

    public async Task AddAsync({moduleName} entity, CancellationToken ct = default)
    {{
        await _context.Set<{moduleName}>().AddAsync(entity, ct);
    }}

    public void Delete({moduleName} entity)
    {{
        _context.Set<{moduleName}>().Remove(entity);
    }}
}}";

    private static string GenerateRepositoryInterfaceCode(string moduleName) => $@"using RVR.Framework.Core.Repositories;
using RVR.Framework.{moduleName}.Domain.Entities;

namespace RVR.Framework.{moduleName}.Domain.Repositories;

/// <summary>
/// Repository interface for {moduleName} aggregate.
/// </summary>
public interface I{moduleName}Repository : IRepository<{moduleName}, Guid>
{{
    Task<IEnumerable<{moduleName}>> GetAllAsync(CancellationToken cancellationToken = default);
}}";

    private static string GenerateEntityConfigurationCode(string moduleName) => $@"using RVR.Framework.{moduleName}.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVR.Framework.{moduleName}.Infrastructure.Configurations;

/// <summary>
/// Entity configuration for {moduleName}.
/// </summary>
public class {moduleName}Configuration : IEntityTypeConfiguration<{moduleName}>
{{
    public void Configure(EntityTypeBuilder<{moduleName}> builder)
    {{
        builder.ToTable(""{moduleName}s"");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsActive);
    }}
}}";

    private static string GenerateInfrastructureCsproj(string moduleName, bool includeMigrations) => $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>RVR.Framework.{moduleName}.Infrastructure</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" />
    {(includeMigrations ? "<PackageReference Include=\"Microsoft.EntityFrameworkCore.Design\" PrivateAssets=\"all\" />" : "")}
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\..\\src\\RVR.Framework.Data\\RVR.Framework.Data.csproj"" />
    <ProjectReference Include=""..\\RVR.Framework.{moduleName}.Domain\\RVR.Framework.{moduleName}.Domain.csproj"" />
  </ItemGroup>

</Project>";

    private static string GenerateControllerCode(string moduleName) => $@"using Microsoft.AspNetCore.Mvc;
using RVR.Framework.Api.Controllers;
using MediatR;
using RVR.Framework.{moduleName}.Application.Features.{moduleName}.Create;
using RVR.Framework.{moduleName}.Application.Features.{moduleName}.Update;
using RVR.Framework.{moduleName}.Application.Features.{moduleName}.GetById;
using RVR.Framework.{moduleName}.Application.Features.{moduleName}.GetAll;
using RVR.Framework.{moduleName}.Application.Features.{moduleName}.Delete;

namespace RVR.Framework.{moduleName}.Api.Controllers;

/// <summary>
/// API Controller for {moduleName} operations.
/// </summary>
[ApiController]
[Route(""api/[controller]"")]
public class {moduleName}Controller : BaseApiController
{{
    public {moduleName}Controller(IMediator mediator) : base(mediator) {{ }}

    /// <summary>
    /// Get all {moduleName} entities.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {{
        var result = await Mediator.Send(new GetAll{moduleName}Query());
        return Ok(result);
    }}

    /// <summary>
    /// Get {moduleName} by ID.
    /// </summary>
    [HttpGet(""{{id:guid}}"")]
    public async Task<IActionResult> GetById(Guid id)
    {{
        var result = await Mediator.Send(new Get{moduleName}ByIdQuery(id));
        return Ok(result);
    }}

    /// <summary>
    /// Create a new {moduleName}.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create{moduleName}Command command)
    {{
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new {{ id = result.Id }}, result);
    }}

    /// <summary>
    /// Update an existing {moduleName}.
    /// </summary>
    [HttpPut(""{{id:guid}}"")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update{moduleName}Command command)
    {{
        command = command with {{ Id = id }};
        var result = await Mediator.Send(command);
        return Ok(result);
    }}

    /// <summary>
    /// Delete a {moduleName}.
    /// </summary>
    [HttpDelete(""{{id:guid}}"")]
    public async Task<IActionResult> Delete(Guid id)
    {{
        await Mediator.Send(new Delete{moduleName}Command(id));
        return NoContent();
    }}
}}";

    private static string GenerateApiCsproj(string moduleName) => $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>RVR.Framework.{moduleName}.Api</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include=""Microsoft.AspNetCore.App"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\..\\src\\RVR.Framework.Api\\RVR.Framework.Api.csproj"" />
    <ProjectReference Include=""..\\RVR.Framework.{moduleName}.Application\\RVR.Framework.{moduleName}.Application.csproj"" />
  </ItemGroup>

</Project>";

    private static string GenerateDomainTestsCode(string moduleName) => $@"using RVR.Framework.{moduleName}.Domain.Entities;

namespace RVR.Framework.{moduleName}.Tests.Domain;

/// <summary>
/// Unit tests for {moduleName} domain entity.
/// </summary>
public class {moduleName}Tests
{{
    [Fact]
    public void Create_ShouldSetProperties()
    {{
        // Arrange
        var name = ""Test {moduleName}"";
        var description = ""Test description"";

        // Act
        var entity = {moduleName}.Create(name, description);

        // Assert
        Assert.Equal(name, entity.Name);
        Assert.Equal(description, entity.Description);
        Assert.True(entity.IsActive);
        Assert.NotEqual(Guid.Empty, entity.Id);
    }}

    [Fact]
    public void Update_ShouldModifyProperties()
    {{
        // Arrange
        var entity = {moduleName}.Create(""Original"", ""Original desc"");
        var newName = ""Updated name"";
        var newDesc = ""Updated description"";

        // Act
        entity.Update(newName, newDesc);

        // Assert
        Assert.Equal(newName, entity.Name);
        Assert.Equal(newDesc, entity.Description);
        Assert.True(entity.UpdatedAt > entity.CreatedAt);
    }}
}}";

    private static string GenerateApplicationTestsCode(string moduleName) => $@"using NSubstitute;
using RVR.Framework.{moduleName}.Application.Features.{moduleName}.Create;
using RVR.Framework.{moduleName}.Domain.Repositories;
using RVR.Framework.{moduleName}.Domain.Entities;

namespace RVR.Framework.{moduleName}.Tests.Application;

/// <summary>
/// Unit tests for {moduleName} application handlers.
/// </summary>
public class {moduleName}HandlerTests
{{
    private readonly I{moduleName}Repository _repository;
    private readonly Create{moduleName}CommandHandler _handler;

    public {moduleName}HandlerTests()
    {{
        _repository = Substitute.For<I{moduleName}Repository>();
        _handler = new Create{moduleName}CommandHandler(_repository);
    }}

    [Fact]
    public async Task Handle_ShouldCreate{moduleName}()
    {{
        // Arrange
        var command = new Create{moduleName}Command(""Test"", ""Description"");
        _repository.UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(""Test"", result.Name);
        await _repository.Received(1).AddAsync(Arg.Any<{moduleName}>(), Arg.Any<CancellationToken>());
    }}
}}";

    private static string GenerateTestCsproj(string moduleName) => $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <RootNamespace>RVR.Framework.{moduleName}.Tests</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" />
    <PackageReference Include=""xunit"" />
    <PackageReference Include=""xunit.runner.visualstudio"" PrivateAssets=""all"" />
    <PackageReference Include=""NSubstitute"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\..\\src\\RVR.Framework.{moduleName}.Application\\RVR.Framework.{moduleName}.Application.csproj"" />
  </ItemGroup>

</Project>";
}
