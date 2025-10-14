namespace RVR.Framework.Application.DTOs.Tenants;

/// <summary>
/// DTO pour le tenant
/// </summary>
public record TenantDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt
);
