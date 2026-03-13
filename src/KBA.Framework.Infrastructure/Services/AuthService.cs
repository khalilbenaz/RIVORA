using KBA.Framework.Application.DTOs.Auth;
using KBA.Framework.Application.Services;
using KBA.Framework.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace KBA.Framework.Infrastructure.Services;

/// <summary>
/// Service d'authentification
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        JwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        // Récupérer l'utilisateur
        var user = await _userRepository.GetByUserNameAsync(loginDto.UserName, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Tentative de connexion avec un nom d'utilisateur invalide: {UserName}", loginDto.UserName);
            throw new UnauthorizedAccessException("Nom d'utilisateur ou mot de passe invalide.");
        }

        // Vérifier le mot de passe (Note: Dans une vraie application, utiliser un système de hachage approprié)
        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Tentative de connexion avec un mot de passe invalide pour l'utilisateur: {UserName}", loginDto.UserName);
            throw new UnauthorizedAccessException("Nom d'utilisateur ou mot de passe invalide.");
        }

        // Vérifier si l'utilisateur est actif
        if (!user.IsActive)
        {
            _logger.LogWarning("Tentative de connexion avec un compte désactivé: {UserName}", loginDto.UserName);
            throw new UnauthorizedAccessException("Ce compte est désactivé.");
        }

        // Générer les tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Mettre à jour le refresh token dans la base de données
        // (Note: Vous devez implémenter le stockage des refresh tokens)

        _logger.LogInformation("Utilisateur connecté avec succès: {UserName}", loginDto.UserName);

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60), // À synchroniser avec la configuration JWT
            user.UserName,
            user.Email
        );
    }

    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Implémenter la logique de rafraîchissement du token
        // Vérifier que le refresh token est valide et non expiré
        // Générer un nouveau access token
        throw new NotImplementedException("La fonctionnalité de rafraîchissement du token n'est pas encore implémentée.");
    }

    public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Implémenter la logique de déconnexion
        // Révoquer le refresh token
        throw new NotImplementedException("La fonctionnalité de déconnexion n'est pas encore implémentée.");
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Note: Ceci est une implémentation simplifiée
        // Dans une vraie application, utiliser BCrypt, Argon2, ou un système similaire
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToBase64String(hashedBytes);
        return hash == passwordHash;
    }
}
