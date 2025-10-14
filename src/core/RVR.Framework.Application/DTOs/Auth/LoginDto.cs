namespace RVR.Framework.Application.DTOs.Auth;

/// <summary>
/// DTO pour la connexion
/// </summary>
public record LoginDto(
    string UserName,
    string Password
);
