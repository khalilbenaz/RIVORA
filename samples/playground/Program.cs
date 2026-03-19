var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RIVORA Playground API",
        Version = "v1",
        Description = "A minimal playground to explore the RIVORA Framework"
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Endpoints ---
app.MapGet("/api/hello", () => Results.Ok(new
{
    Message = "Hello from RIVORA Framework!",
    Framework = "RIVORA",
    Timestamp = DateTime.UtcNow
}))
.WithName("Hello")
.WithTags("Demo")
.WithOpenApi();

app.MapGet("/api/hello/{name}", (string name) => Results.Ok(new
{
    Message = $"Hello, {name}! Welcome to RIVORA.",
    Timestamp = DateTime.UtcNow
}))
.WithName("HelloName")
.WithTags("Demo")
.WithOpenApi();

app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "Healthy",
    Framework = "RIVORA Framework",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
}))
.WithName("Health")
.WithTags("Health")
.WithOpenApi();

app.MapHealthChecks("/healthz");

app.Run();
