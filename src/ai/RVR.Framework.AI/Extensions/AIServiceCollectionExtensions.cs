using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.AI.Extensions;

/// <summary>
/// Extension methods for registering RVR.Framework.AI services in the dependency injection container.
/// </summary>
public static class AIServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core AI services including the in-memory vector store, sliding window chunker,
    /// and RAG service. Consumers must register their own <see cref="IChatClient"/> and
    /// <see cref="IEmbeddingService"/> implementations.
    /// </summary>
    public static IServiceCollection AddRvrAI(this IServiceCollection services)
    {
        services.AddSingleton<IVectorStore, InMemoryVectorStore>();
        services.AddSingleton<IDocumentChunker, SlidingWindowChunker>();
        services.AddScoped<IRagService, RagService>();
        return services;
    }
}
