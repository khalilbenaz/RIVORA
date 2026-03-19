using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.AI.Guardrails.Extensions;
using RVR.Framework.Core.Modules;

namespace RVR.Framework.AI.Guardrails;

/// <summary>
/// RIVORA module for AI Guardrails. Provides automatic registration
/// via the standard module discovery mechanism.
/// </summary>
public sealed class GuardrailsModule : IRvrModule
{
    /// <inheritdoc />
    public string Name => "AI.Guardrails";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRvrAIGuardrails(configuration);
    }

    /// <inheritdoc />
    public void Configure(IApplicationBuilder app)
    {
        // No middleware registration required; guardrails are invoked
        // through the IGuardrailPipeline service within AI workflows.
    }
}
