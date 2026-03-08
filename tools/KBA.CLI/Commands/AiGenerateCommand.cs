using Spectre.Console;
using System.Text;

namespace KBA.CLI.Commands;

/// <summary>
/// Provides AI-powered code generation functionality.
/// </summary>
public static class AiGenerateCommand
{
    private static string? _apiKey;
    private static string _provider = "openai";
    private static string _model = "gpt-4o";

    /// <summary>
    /// Executes the AI code generation command.
    /// </summary>
    /// <param name="prompt">The generation prompt describing what to generate.</param>
    /// <param name="output">Output file path (optional).</param>
    /// <param name="provider">The LLM provider (openai, claude).</param>
    /// <param name="model">The model to use.</param>
    /// <param name="apiKey">The API key for the provider.</param>
    /// <param name="language">Target programming language (default: csharp).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ExecuteAsync(
        string prompt, 
        string? output, 
        string provider, 
        string model, 
        string? apiKey,
        string language = "csharp")
    {
        AnsiConsole.Write(new FigletText("AI Generate").Color(Color.Cyan1));
        AnsiConsole.MarkupLine("[grey]AI-powered code generation[/]" + Environment.NewLine);

        _provider = provider.ToLower();
        _model = string.IsNullOrWhiteSpace(model) ? GetDefaultModel(_provider) : model;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable(GetApiKeyEnvVar(_provider));

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            AnsiConsole.MarkupLine("[bold red]Error:[/] API key not provided.");
            AnsiConsole.MarkupLine($"Set {GetApiKeyEnvVar(_provider)} environment variable or use --api-key option.");
            return;
        }

        var generationConfig = new
        {
            Provider = _provider,
            Model = _model,
            Language = language,
            Prompt = prompt
        };

        var configTable = new Table();
        configTable.AddColumn("Setting");
        configTable.AddColumn("Value");
        configTable.AddRow("[cyan]Provider[/]", _provider);
        configTable.AddRow("[cyan]Model[/]", _model);
        configTable.AddRow("[cyan]Language[/]", language);
        configTable.AddRow("[cyan]Output[/]", output ?? "Console");
        AnsiConsole.Write(configTable);
        AnsiConsole.WriteLine();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Generating code...", async ctx =>
            {
                try
                {
                    var generatedCode = await GenerateCodeAsync(prompt, language);
                    
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        await WriteToFileAsync(output, generatedCode);
                        AnsiConsole.MarkupLine($"[bold green]✓ Code written to {output}[/]");
                    }
                    else
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold blue]Generated Code:[/]");
                        AnsiConsole.Write(new Panel(generatedCode)
                        {
                            Header = new PanelHeader($"[cyan]{language}[/]"),
                            Border = BoxBorder.Rounded,
                            Padding = new Padding(1, 0, 1, 0)
                        });
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
                }
            });

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

    private static async Task<string> GenerateCodeAsync(string prompt, string language)
    {
        // Simulated code generation - in production, integrate with actual API
        await Task.Delay(1000);

        var languageExt = language.ToLower() switch
        {
            "csharp" => "cs",
            "javascript" => "js",
            "typescript" => "ts",
            "python" => "py",
            _ => "txt"
        };

        // Generate sample code based on prompt keywords
        var code = prompt.ToLower() switch
        {
            var p when p.Contains("aggregate") => GenerateAggregateCode(),
            var p when p.Contains("controller") => GenerateControllerCode(),
            var p when p.Contains("service") => GenerateServiceCode(),
            var p when p.Contains("repository") => GenerateRepositoryCode(),
            var p when p.Contains("entity") => GenerateEntityCode(),
            _ => GenerateGenericCode(prompt, languageExt)
        };

        return code;
    }

    private static string GenerateAggregateCode()
    {
        return @"using KBA.Framework.Core.Aggregates;
using KBA.Framework.Core.Events;

namespace KBA.Framework.Domain.Entities;

/// <summary>
/// Sample aggregate root demonstrating KBA Framework patterns.
/// </summary>
public class ProductAggregate : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public int StockQuantity { get; private set; }

    private ProductAggregate() { }

    public static ProductAggregate Create(string name, string description, decimal price)
    {
        var aggregate = new ProductAggregate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            IsActive = true,
            StockQuantity = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        aggregate.AddEvent(new DomainEvent(""ProductCreated"", aggregate.Id));
        return aggregate;
    }

    public void Update(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
        AddEvent(new DomainEvent(""ProductUpdated"", Id));
    }

    public void SetStockQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException(""Stock quantity cannot be negative"");
        
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}";
    }

    private static string GenerateControllerCode()
    {
        return @"using Microsoft.AspNetCore.Mvc;
using KBA.Framework.Api.Controllers;
using MediatR;

namespace KBA.Framework.Api.Controllers;

/// <summary>
/// API Controller for Product operations.
/// </summary>
[ApiController]
[Route(""api/[controller]"")]
public class ProductsController : BaseApiController
{
    public ProductsController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get all products.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllProductsQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID.
    /// </summary>
    [HttpGet(""{id:guid}"")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    [HttpPut(""{id:guid}"")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        command.Id = id;
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete a product.
    /// </summary>
    [HttpDelete(""{id:guid}"")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand(id);
        await Mediator.Send(command);
        return NoContent();
    }
}";
    }

    private static string GenerateServiceCode()
    {
        return @"using KBA.Framework.Application.Services;
using KBA.Framework.Core.Repositories;

namespace KBA.Framework.Application.Services;

/// <summary>
/// Product service implementing business logic.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation(""Getting product by ID: {Id}"", id);
        var product = await _repository.GetByIdAsync(id, ct);
        
        if (product == null)
            throw new NotFoundException($""Product with ID {id} not found"");

        return MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation(""Getting all products"");
        var products = await _repository.GetAllAsync(ct);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation(""Creating new product: {Name}"", dto.Name);
        
        var product = ProductAggregate.Create(dto.Name, dto.Description, dto.Price);
        await _repository.AddAsync(product, ct);
        await _repository.UnitOfWork.SaveChangesAsync(ct);

        return MapToDto(product);
    }

    private static ProductDto MapToDto(ProductAggregate product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        IsActive = product.IsActive,
        StockQuantity = product.StockQuantity,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}";
    }

    private static string GenerateRepositoryCode()
    {
        return @"using KBA.Framework.Core.Repositories;
using KBA.Framework.Data;
using Microsoft.EntityFrameworkCore;

namespace KBA.Framework.Infrastructure.Repositories;

/// <summary>
/// Entity Framework repository for Product aggregate.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<ProductAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<ProductAggregate>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ProductAggregate product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
    }

    public void Update(ProductAggregate product)
    {
        _context.Products.Update(product);
    }

    public void Delete(ProductAggregate product)
    {
        _context.Products.Remove(product);
    }
}";
    }

    private static string GenerateEntityCode()
    {
        return @"using KBA.Framework.Core.Entities;

namespace KBA.Framework.Domain.Entities;

/// <summary>
/// Product entity representing a sellable item.
/// </summary>
public class Product : Entity<Guid>, IAuditable, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    
    // Auditable
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    // Soft Deletable
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}";
    }

    private static string GenerateGenericCode(string prompt, string languageExt)
    {
        return $@"// Generated code for: {prompt}
// Language: {languageExt}
// Provider: {_provider}
// Model: {_model}

// This is a placeholder for AI-generated code.
// In production mode, this would be generated by the {_model} model.

public class GeneratedClass
{{
    public string Description {{ get; set; }} = ""{prompt}"";
    public DateTime GeneratedAt {{ get; set; }} = DateTime.UtcNow;
    
    public void Execute()
    {{
        // Implementation generated by AI
        Console.WriteLine(""Executing generated code..."");
    }}
}}";
    }

    private static async Task WriteToFileAsync(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(path, content);
    }
}
