namespace RVR.Framework.Api.Interfaces;

/// <summary>
/// Interface pour mapper des endpoints Minimal API (7.4)
/// </summary>
public interface IMapEndpoints
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
