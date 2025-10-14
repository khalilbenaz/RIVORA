namespace RVR.Framework.Application.DTOs.Products;

/// <summary>
/// DTO pour la mise à jour d'un produit
/// </summary>
public record UpdateProductDto(
    string Name,
    string? Description,
    decimal Price,
    string? SKU,
    string? Category
);
