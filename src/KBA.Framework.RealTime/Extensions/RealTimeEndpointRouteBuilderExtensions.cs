using KBA.Framework.RealTime.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace KBA.Framework.RealTime.Extensions;

public static class RealTimeEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapKbaHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<KbaHub>("/hubs/kba");
        
        return endpoints;
    }
}
