using RVR.Framework.Application.DTOs.Auth;
using RVR.Framework.Application.Services;
using RVR.Framework.Domain.Repositories;
using RVR.Framework.Infrastructure.Configuration;
using RVR.Framework.Security.Interfaces;
using RVR.Framework.Security.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RVR.Framework.Infrastructure.Services;

/// <summary>
/// Service d'authentification avec support du verrouillage de compte (anti brute-force)
/// et stockage persistant des refresh tokens.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AccountLockoutOptions _lockoutOptions;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        JwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthService> logger,
        IPasswordHasher passwordHasher,
        IOptions<AccountLockoutOptions> lockoutOptions,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _lockoutOptions = lockoutOptions.Value;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        // Récupérer l'utilisateur
        var user = await _userRepository.GetByUserNameAsync(loginDto.UserName, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Tentative de connexion avec un nom d'utilisateur invalide: {UserName}", SanitizeLogInput(loginDto.UserName));
            throw new UnauthorizedAccessException("Nom d'utilisateur ou mot de passe invalide.");
        }

        // Vérifier si le compte est verrouillé (anti brute-force)
        if (_lockoutOptions.EnableLockout && user.IsLockedOut())
        {
            _logger.LogWarning(
                "Tentative de connexion sur un compte verrouillé: {UserName}. Verrouillé jusqu'à {LockoutEndUtc}",
                SanitizeLogInput(loginDto.UserName), user.LockoutEndUtc);
            throw new UnauthorizedAccessException(
                $"Compte verrouillé. Réessayez après {user.LockoutEndUtc:yyyy-MM-dd HH:mm:ss} UTC");
        }

        // Vérifier le mot de passe
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Tentative de connexion avec un mot de passe invalide pour l'utilisateur: {UserName}", SanitizeLogInput(loginDto.UserName));

            // Incrémenter les tentatives échouées et verrouiller si nécessaire
            if (_lockoutOptions.EnableLockout)
            {
                user.IncrementFailedLogins();

                if (user.FailedLoginAttempts >= _lockoutOptions.MaxFailedAttempts)
                {
                    var lockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutOptions.LockoutDurationMinutes);
                    user.LockUntil(lockoutEnd);
                    _logger.LogWarning(
                        "Compte verrouillé après {FailedAttempts} tentatives échouées: {UserName}. Verrouillé jusqu'à {LockoutEnd}",
                        user.FailedLoginAttempts, SanitizeLogInput(loginDto.UserName), lockoutEnd);
                }

                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            throw new UnauthorizedAccessException("Nom d'utilisateur ou mot de passe invalide.");
        }

        // Vérifier si l'utilisateur est actif
        if (!user.IsActive)
        {
            _logger.LogWarning("Tentative de connexion avec un compte désactivé: {UserName}", SanitizeLogInput(loginDto.UserName));
            throw new UnauthorizedAccessException("Ce compte est désactivé.");
        }

        // Connexion réussie : réinitialiser les tentatives échouées
        if (_lockoutOptions.EnableLockout && user.FailedLoginAttempts > 0)
        {
            user.ResetFailedLogins();
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        // Générer le token d'accès
        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        // Générer et persister le refresh token via le service dédié
        var refreshToken = await _refreshTokenService.GenerateAsync(
            user.Id.ToString(),
            ipAddress: "unknown",
            tenantId: user.TenantId?.ToString(),
            cancellationToken: cancellationToken);

        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        _logger.LogInformation("Utilisateur connecté avec succès: {UserName}", SanitizeLogInput(loginDto.UserName));

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(expirationMinutes),
            user.UserName,
            user.Email
        );
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Le refresh token est requis.");
        }

        // Validate and rotate the refresh token via the persistent store
        var result = await _refreshTokenService.ValidateAndRotateAsync(
            refreshToken,
            ipAddress: "unknown",
            cancellationToken);

        if (!result.IsValid)
        {
            _logger.LogWarning("Tentative de rafraîchissement avec un token invalide: {Error}", result.ErrorMessage);
            throw new UnauthorizedAccessException(result.ErrorMessage ?? "Le refresh token est invalide ou expiré.");
        }

        // Look up the user from the validated result
        if (!Guid.TryParse(result.UserId, out var userId))
        {
            throw new UnauthorizedAccessException("Impossible d'identifier l'utilisateur depuis le token.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Utilisateur introuvable ou désactivé.");
        }

        // Générer un nouveau token d'accès
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        _logger.LogInformation("Token rafraîchi avec succès pour l'utilisateur: {UserName}", user.UserName);

        return new AuthResponseDto(
            newAccessToken,
            result.NewRefreshToken!,
            DateTime.UtcNow.AddMinutes(expirationMinutes),
            user.UserName,
            user.Email
        );
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Tentative de déconnexion sans refresh token");
            return;
        }

        // Revoke the refresh token in the persistent store
        await _refreshTokenService.RevokeAsync(
            refreshToken,
            reason: "User logout",
            ipAddress: "unknown",
            cancellationToken);

        _logger.LogInformation("Déconnexion réussie, refresh token révoqué");
    }

    /// <summary>
    /// Sanitize user input before logging to prevent log injection attacks.
    /// Removes newlines, carriage returns, and other control characters.
    /// </summary>
    private static string SanitizeLogInput(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "[empty]";
        return input
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("\t", "")
            .Replace("\0", "");
    }
}
