using Microsoft.Extensions.DependencyInjection;

namespace KBA.Framework.FeatureManagement;

public static class FeatureManagementServiceCollectionExtensions
{
    public static IServiceCollection AddKbaFeatureManagement(this IServiceCollection services)
    {
        // TODO: Register Feature Management Services and DB Contexts here.
        return services;
    }
}