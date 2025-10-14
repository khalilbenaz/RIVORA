namespace RVR.Framework.GraphQL.Mutations;

using RVR.Framework.Application.Services;
using RVR.Framework.Application.DTOs.Products;
using RVR.Framework.Application.DTOs.Auth;

/// <summary>
/// Root mutation type for the GraphQL schema.
/// </summary>
public class Mutation
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    public async Task<AuthResponseDto> Login(
        [Service] IAuthService authService,
        string userName, string password,
        CancellationToken ct)
    {
        return await authService.LoginAsync(new LoginDto(userName, password), ct);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public async Task<ProductDto> CreateProduct(
        [Service] IProductService productService,
        CreateProductDto input,
        CancellationToken ct)
    {
        return await productService.CreateAsync(input, ct);
    }
}
