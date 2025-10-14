namespace RVR.Framework.GraphQL.Queries;

using RVR.Framework.Domain.Entities.Identity;
using RVR.Framework.Domain.Entities.Products;
using RVR.Framework.Infrastructure.Data;
using HotChocolate;
using HotChocolate.Data;

/// <summary>
/// Root query type for the GraphQL schema.
/// </summary>
public class Query
{
    /// <summary>
    /// Gets all products with support for projection, filtering, and sorting.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts([Service] RVRDbContext context)
        => context.Products;

    /// <summary>
    /// Gets all users with support for projection, filtering, and sorting.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] RVRDbContext context)
        => context.Users;

    /// <summary>
    /// Gets a single product by its identifier.
    /// </summary>
    public async Task<Product?> GetProductById([Service] RVRDbContext context, Guid id)
        => await context.Products.FindAsync(id);

    /// <summary>
    /// Gets a single user by its identifier.
    /// </summary>
    public async Task<User?> GetUserById([Service] RVRDbContext context, Guid id)
        => await context.Users.FindAsync(id);
}
