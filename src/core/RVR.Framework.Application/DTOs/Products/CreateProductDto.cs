namespace RVR.Framework.Application.DTOs.Products;

/// <summary>
/// DTO pour la création d'un produit
/// </summary>
public record CreateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? SKU,
    string? Category
);
