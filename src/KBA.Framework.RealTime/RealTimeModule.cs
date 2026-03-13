using KBA.Framework.Core.Modules;
using KBA.Framework.RealTime.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KBA.Framework.RealTime;

public class RealTimeModule : IKbaModule
{
    public string Name => "RealTime";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddKbaRealTime();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Les hubs sont mappés via MapKbaHubs dans Program.cs ou via IMapEndpoints
    }
}
