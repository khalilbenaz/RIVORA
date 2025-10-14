namespace RVR.Framework.Application.DTOs.Auth;

/// <summary>
/// DTO pour la réponse d'authentification
/// </summary>
public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserName,
    string Email
);
