using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Core.Modules;
using RVR.Framework.SMS.Extensions;

namespace RVR.Framework.SMS;

/// <summary>
/// RIVORA Framework module for multi-provider SMS messaging.
/// Implements <see cref="IRvrModule"/> for automatic module discovery and registration.
/// </summary>
public sealed class SmsModule : IRvrModule
{
    /// <inheritdoc />
    public string Name => "SMS";

    /// <summary>
    /// Gets the module version.
    /// </summary>
    public string Version => "3.1.0";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => services.AddRvrSms(configuration);

    /// <inheritdoc />
    public void Configure(IApplicationBuilder app)
    {
        // No middleware pipeline configuration required for the SMS module.
    }
}
