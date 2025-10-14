namespace RVR.Framework.Application.DTOs.Users;

/// <summary>
/// DTO pour la création d'un utilisateur
/// </summary>
public record CreateUserDto(
    string UserName,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? PhoneNumber
);
