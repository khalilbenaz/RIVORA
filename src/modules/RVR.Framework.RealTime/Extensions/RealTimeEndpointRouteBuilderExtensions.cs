using RVR.Framework.RealTime.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace RVR.Framework.RealTime.Extensions;

public static class RealTimeEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapRvrHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<KbaHub>("/hubs/kba");

        return endpoints;
    }
}
