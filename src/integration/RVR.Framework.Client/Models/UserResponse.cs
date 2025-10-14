namespace RVR.Framework.Client.Models;

/// <summary>
/// User response DTO.
/// </summary>
public record UserResponse(
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
