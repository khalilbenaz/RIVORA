namespace RVR.Framework.GraphQL.Extensions;

using RVR.Framework.GraphQL.Queries;
using RVR.Framework.GraphQL.Mutations;
using RVR.Framework.GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for registering GraphQL services in the DI container.
/// </summary>
public static class GraphQLServiceCollectionExtensions
{
    /// <summary>
    /// Adds Rivora GraphQL server configuration to the service collection.
    /// </summary>
    public static IServiceCollection AddRvrGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddType<UserType>()
            .AddType<ProductType>()
            .AddProjections()
            .AddFiltering()
            .AddSorting();

        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// Maps the GraphQL endpoint to the specified path.
    /// </summary>
    public static WebApplication MapRvrGraphQL(this WebApplication app, string path = "/graphql")
    {
        app.MapGraphQL(path);
        return app;
    }
}
