using Serilog;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using KBA.Microservices.Shared.Domain.Entities;
using KBA.Microservices.Shared.Application.Events;
using KBA.Microservices.Shared.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=postgres_products;Database=products_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddGrpc();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<IEventBus, MassTransitEventBus>();

builder.Services.AddCors(options => options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.MapGrpcService<ProductGrpcService>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await db.Database.MigrateAsync();
    await SeedDataAsync(db);
}

Log.Information("Starting ProductService");
app.Run();

static async Task SeedDataAsync(ProductDbContext db)
{
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 50, Sku = "LAP-001" },
            new Product { Id = Guid.NewGuid(), Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Stock = 200, Sku = "MOU-001" },
            new Product { Id = Guid.NewGuid(), Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, Stock = 100, Sku = "KEY-001" }
        );
        await db.SaveChangesAsync();
    }
}

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sku).IsUnique();
        });
    }
}

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ProductDbContext _db;
    private readonly IEventBus _eventBus;

    public ProductGrpcService(ProductDbContext db, IEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public override async Task<ProductResponse> GetProductById(ProductRequest request, ServerCallContext context)
    {
        var product = await _db.Products.FindAsync(Guid.Parse(request.ProductId));
        if (product == null) return new ProductResponse();

        return new ProductResponse
        {
            ProductId = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price,
            Stock = product.Stock,
            IsAvailable = product.Stock > 0 && product.IsActive
        };
    }

    public override async Task<ReserveStockResponse> ReserveStock(ReserveStockRequest request, ServerCallContext context)
    {
        var product = await _db.Products.FindAsync(Guid.Parse(request.ProductId));
        if (product == null || product.Stock < request.Quantity)
            return new ReserveStockResponse { Success = false, Message = "Insufficient stock" };

        product.Stock -= request.Quantity;
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync(new ProductStockUpdatedEvent
        {
            ProductId = product.Id,
            OldStock = product.Stock + request.Quantity,
            NewStock = product.Stock
        });

        return new ReserveStockResponse { Success = true, Message = "Stock reserved" };
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(ReleaseStockRequest request, ServerCallContext context)
    {
        var product = await _db.Products.FindAsync(Guid.Parse(request.ProductId));
        if (product == null)
            return new ReleaseStockResponse { Success = false, Message = "Product not found" };

        product.Stock += request.Quantity;
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync(new ProductStockUpdatedEvent
        {
            ProductId = product.Id,
            OldStock = product.Stock - request.Quantity,
            NewStock = product.Stock
        });

        return new ReleaseStockResponse { Success = true, Message = "Stock released" };
    }
}
