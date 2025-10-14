namespace RVR.Framework.Application.DTOs.Users;

/// <summary>
/// DTO pour l'utilisateur
/// </summary>
public record UserDto(
    Guid Id,
    Guid? TenantId,
    string UserName,
    string Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled,
    bool IsActive,
    string? FirstName,
    string? LastName,
    string FullName,
    DateTime CreatedAt
);
