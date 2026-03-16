using KBA.Framework.Application.DTOs.Auth;
using KBA.Framework.Application.Services;
using KBA.Framework.Domain.Repositories;
using KBA.Framework.Security.Services;
using Microsoft.Extensions.Logging;

namespace KBA.Framework.Infrastructure.Services;

/// <summary>
/// Service d'authentification
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        JwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _passwordHasher = passwordHasher;
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

        // Vérifier le mot de passe
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
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
}
