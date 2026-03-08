using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KBA.AI.RAG.App.AI;
using KBA.AI.RAG.App.VectorStore;
using KBA.AI.RAG.App.RAG;

namespace KBA.AI.RAG.App.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=ai_rag;Username=postgres;Password=postgres";
        services.AddDbContext<AiRagDbContext>(options => options.UseNpgsql(connectionString));

        // AI Providers
        services.Configure<OpenAiSettings>(configuration.GetSection("AIProviders:OpenAI"));
        services.Configure<AnthropicSettings>(configuration.GetSection("AIProviders:Anthropic"));
        services.Configure<OllamaSettings>(configuration.GetSection("AIProviders:Ollama"));

        services.AddHttpClient<OpenAiProvider>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<OpenAiSettings>>().Value;
            client.BaseAddress = new Uri(settings.Endpoint);
        });

        services.AddHttpClient<AnthropicProvider>((sp, client) =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com");
        });

        services.AddHttpClient<OllamaProvider>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<OllamaSettings>>().Value;
            client.BaseAddress = new Uri(settings.Endpoint);
        });

        services.AddScoped<IAiProvider>(sp => sp.GetRequiredService<OpenAiProvider>());

        // Vector Store
        services.Configure<QdrantSettings>(configuration.GetSection("VectorStore:Qdrant"));
        services.AddHttpClient<QdrantVectorStore>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
            client.BaseAddress = new Uri(settings.Endpoint);
        });
        services.AddScoped<IVectorStore>(sp => sp.GetRequiredService<QdrantVectorStore>());

        // RAG Pipeline
        services.AddScoped<IRagPipeline, RagPipeline>();

        return services;
    }
}

public class AiRagDbContext : DbContext
{
    public AiRagDbContext(DbContextOptions<AiRagDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<TokenUsage> TokenUsages => Set<TokenUsage>();
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Messages).WithOne();
        });

        modelBuilder.Entity<TokenUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.UsageDate);
        });
    }
}
