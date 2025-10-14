using RVR.Framework.Api.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Découverte automatique et mapping des endpoints Minimal API (7.4)
    /// </summary>
    public static void MapRvrEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointMappers = app.ServiceProvider.GetServices<IMapEndpoints>();
        foreach (var mapper in endpointMappers)
        {
            mapper.MapEndpoints(app);
        }
    }
}
