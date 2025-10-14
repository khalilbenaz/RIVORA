using RVR.Framework.Application.DTOs.Auth;

namespace RVR.Framework.Application.Services;

/// <summary>
/// Interface pour le service d'authentification
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authentifie un utilisateur et génère un token JWT
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rafraîchit un token JWT
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Déconnecte un utilisateur (révoque le refresh token)
    /// </summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
