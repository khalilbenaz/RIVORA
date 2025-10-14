namespace RVR.Framework.Application.DTOs.Products;

/// <summary>
/// DTO pour le produit
/// </summary>
public record ProductDto(
    Guid Id,
    Guid? TenantId,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool IsActive,
    string? SKU,
    string? Category,
    DateTime CreatedAt,
    Guid? CreatorId,
    DateTime? UpdatedAt,
    Guid? LastModifierId
);
