using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using RVR.AOT.Minimal;

var builder = WebApplication.CreateSlimBuilder(args);

// Configure JSON serialization with source generators (AOT-compatible)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

var app = builder.Build();

// --- Health Endpoint ---
app.MapGet("/health", () => TypedResults.Ok(new HealthResponse(
    Status: "Healthy",
    Framework: "RIVORA Framework",
    AotEnabled: true,
    Timestamp: DateTime.UtcNow
)));

// --- Product CRUD with in-memory storage ---
var products = new ConcurrentDictionary<Guid, Product>();

// Seed sample data
var sampleId = Guid.NewGuid();
products[sampleId] = new Product(sampleId, "RIVORA Enterprise License", 999.99m, "Software");

app.MapGet("/api/products", () =>
{
    var list = products.Values.ToList();
    return TypedResults.Ok(list);
});

app.MapGet("/api/products/{id:guid}", (Guid id) =>
{
    return products.TryGetValue(id, out var product)
        ? Results.Ok(product)
        : Results.NotFound(new ProblemResponse("Product not found", 404));
});

app.MapPost("/api/products", (CreateProductRequest request) =>
{
    var id = Guid.NewGuid();
    var product = new Product(id, request.Name, request.Price, request.Category);

    if (!products.TryAdd(id, product))
    {
        return Results.Problem("Failed to create product", statusCode: 500);
    }

    return Results.Created($"/api/products/{id}", product);
});

app.MapPut("/api/products/{id:guid}", (Guid id, UpdateProductRequest request) =>
{
    if (!products.TryGetValue(id, out var existing))
    {
        return Results.NotFound(new ProblemResponse("Product not found", 404));
    }

    var updated = existing with
    {
        Name = request.Name ?? existing.Name,
        Price = request.Price ?? existing.Price,
        Category = request.Category ?? existing.Category
    };

    products[id] = updated;
    return Results.Ok(updated);
});

app.MapDelete("/api/products/{id:guid}", (Guid id) =>
{
    return products.TryRemove(id, out _)
        ? Results.NoContent()
        : Results.NotFound(new ProblemResponse("Product not found", 404));
});

app.Run();

// --- Models (records for immutability, AOT-friendly) ---
namespace RVR.AOT.Minimal
{
    public record Product(Guid Id, string Name, decimal Price, string Category);

    public record CreateProductRequest(string Name, decimal Price, string Category);

    public record UpdateProductRequest(string? Name, decimal? Price, string? Category);

    public record HealthResponse(string Status, string Framework, bool AotEnabled, DateTime Timestamp);

    public record ProblemResponse(string Detail, int Status);

    // --- JSON Source Generator Context (required for Native AOT) ---
    [JsonSerializable(typeof(Product))]
    [JsonSerializable(typeof(List<Product>))]
    [JsonSerializable(typeof(CreateProductRequest))]
    [JsonSerializable(typeof(UpdateProductRequest))]
    [JsonSerializable(typeof(HealthResponse))]
    [JsonSerializable(typeof(ProblemResponse))]
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    public partial class AppJsonContext : JsonSerializerContext;
}
