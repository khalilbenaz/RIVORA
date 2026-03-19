using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.AI.Agents.Extensions;
using RVR.Framework.Core.Modules;

namespace RVR.Framework.AI.Agents;

/// <summary>
/// RIVORA module for AI Agents. Provides automatic registration
/// via the standard module discovery mechanism.
/// </summary>
public sealed class AgentsModule : IRvrModule
{
    /// <inheritdoc />
    public string Name => "AI.Agents";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRvrAIAgents(configuration);
    }

    /// <inheritdoc />
    public void Configure(IApplicationBuilder app)
    {
        // No middleware registration required; agents are invoked
        // through the IAgent / IAgentPipeline services within AI workflows.
    }
}
