using RVR.Framework.Core.Modules;
using RVR.Framework.RealTime.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.RealTime;

public class RealTimeModule : IRvrModule
{
    public string Name => "RealTime";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRvrRealTime();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Les hubs sont mappés via MapRvrHubs dans Program.cs ou via IMapEndpoints
    }
}
