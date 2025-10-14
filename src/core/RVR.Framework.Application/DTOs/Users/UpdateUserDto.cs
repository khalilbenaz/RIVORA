namespace RVR.Framework.Application.DTOs.Users;

/// <summary>
/// DTO pour la mise à jour d'un utilisateur
/// </summary>
public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? PhoneNumber
);
