namespace RVR.Framework.Client.Models;

/// <summary>
/// Request to update an existing product.
/// </summary>
public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string? SKU,
    string? Category
);
