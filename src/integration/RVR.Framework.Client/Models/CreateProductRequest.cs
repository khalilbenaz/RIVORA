namespace RVR.Framework.Client.Models;

/// <summary>
/// Request to create a new product.
/// </summary>
public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? SKU,
    string? Category
);
