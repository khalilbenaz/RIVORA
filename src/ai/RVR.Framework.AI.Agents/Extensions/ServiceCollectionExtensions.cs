using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.AI.Agents.Agents;
using RVR.Framework.AI.Agents.Configuration;
using RVR.Framework.AI.Agents.Strategies;
using RVR.Framework.AI.Agents.Tools;

namespace RVR.Framework.AI.Agents.Extensions;

/// <summary>
/// Extension methods for registering AI Agents services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RIVORA AI Agents services to the specified <see cref="IServiceCollection"/>.
    /// Registers all built-in agents, tools, and configuration bindings.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration containing agent settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrAIAgents(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(AgentOptions.SectionName);

        // Bind options
        services.Configure<AgentOptions>(section);
        services.Configure<SqlToolOptions>(configuration.GetSection(SqlToolOptions.SectionName));

        // Register built-in agents
        services.AddScoped<SummaryAgent>();
        services.AddScoped<IAgent, SummaryAgent>();
        services.AddScoped<CodeReviewAgent>();
        services.AddScoped<IAgent, CodeReviewAgent>();
        services.AddScoped<DataAnalystAgent>();
        services.AddScoped<IAgent, DataAnalystAgent>();

        // Register ReAct agent
        services.AddScoped<ReActAgent>();
        services.AddScoped<IAgent, ReActAgent>(sp => sp.GetRequiredService<ReActAgent>());

        // Register built-in tools
        services.AddSingleton<ITool, HttpTool>();
        services.AddSingleton<ITool, SqlTool>();
        services.AddSingleton<ITool, FileReadTool>();
        services.AddSingleton<HttpTool>();
        services.AddSingleton<SqlTool>();
        services.AddSingleton<FileReadTool>();

        // Register HTTP client for HttpTool
        services.AddHttpClient("AgentHttpTool");

        return services;
    }
}
