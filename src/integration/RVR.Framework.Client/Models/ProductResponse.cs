namespace RVR.Framework.Client.Models;

/// <summary>
/// Product response DTO.
/// </summary>
public record ProductResponse(
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
