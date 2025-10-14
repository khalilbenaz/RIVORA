namespace RVR.Framework.Security.Services;

using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Entities;
using RVR.Framework.Security.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Configuration options for refresh token service.
/// </summary>
public class RefreshTokenOptions
{
    /// <summary>
    /// Gets or sets the refresh token expiration time in days.
    /// Default is 7 days.
    /// </summary>
    public int ExpirationDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether to enable token rotation.
    /// Default is true for enhanced security.
    /// </summary>
    public bool EnableRotation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to revoke the entire token family on reuse detection.
    /// Default is true for enhanced security.
    /// </summary>
    public bool RevokeOnReuse { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of active refresh tokens per user.
    /// Default is 10.
    /// </summary>
    public int MaxActiveTokensPerUser { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of days to keep revoked tokens before cleanup.
    /// Default is 30 days.
    /// </summary>
    public int RevokedTokenRetentionDays { get; set; } = 30;
}

/// <summary>
/// Service for managing refresh tokens with rotation and revocation support.
/// Implements secure token handling with automatic rotation and reuse detection.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenStore _store;
    private readonly RefreshTokenOptions _options;
    private readonly ILogger<RefreshTokenService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenService"/> class.
    /// </summary>
    /// <param name="store">The refresh token store.</param>
    /// <param name="options">The refresh token options.</param>
    /// <param name="logger">The logger.</param>
    public RefreshTokenService(
        IRefreshTokenStore store,
        IOptions<RefreshTokenOptions> options,
        ILogger<RefreshTokenService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(
        string userId,
        string ipAddress,
        string? tenantId = null,
        string? deviceId = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        var expiresUtc = DateTime.UtcNow.AddDays(_options.ExpirationDays);
        var token = GenerateSecureToken();

        var refreshToken = RefreshToken.Create(
            userId,
            expiresUtc,
            ipAddress,
            tenantId,
            deviceId,
            userAgent);

        refreshToken.Token = token;

        // Check if user has too many active tokens
        var existingTokens = await _store.GetByUserIdAsync(userId, cancellationToken);
        if (existingTokens.Count(t => t.IsActive) >= _options.MaxActiveTokensPerUser)
        {
            // Revoke the oldest token
            var oldestToken = existingTokens
                .Where(t => t.IsActive)
                .OrderBy(t => t.CreatedUtc)
                .FirstOrDefault();

            if (oldestToken != null)
            {
                await _store.RevokeAsync(oldestToken, "Maximum token limit reached", ipAddress, cancellationToken);
                _logger.LogInformation(
                    "Revoked oldest refresh token {TokenId} for user {UserId} due to maximum token limit",
                    oldestToken.Id, userId);
            }
        }

        await _store.StoreAsync(refreshToken, cancellationToken);

        _logger.LogInformation(
            "Generated new refresh token for user {UserId}, expires {ExpiresUtc}",
            userId, expiresUtc);

        return token;
    }

    /// <inheritdoc/>
    public async Task<RefreshTokenValidationResult> ValidateAndRotateAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return RefreshTokenValidationResult.Failure(
                "Refresh token is required.",
                RefreshTokenValidationError.InvalidFormat);
        }

        var token = await _store.GetByTokenAsync(refreshToken, cancellationToken);

        if (token == null)
        {
            return RefreshTokenValidationResult.Failure(
                "Refresh token not found.",
                RefreshTokenValidationError.NotFound);
        }

        // Check if token is revoked
        if (token.IsRevoked)
        {
            _logger.LogWarning(
                "Attempted to use revoked refresh token {TokenId} for user {UserId}",
                token.Id, token.UserId);

            return RefreshTokenValidationResult.Failure(
                "Refresh token has been revoked.",
                RefreshTokenValidationError.Revoked);
        }

        // Check if token is expired
        if (token.IsExpired)
        {
            _logger.LogWarning(
                "Attempted to use expired refresh token {TokenId} for user {UserId}",
                token.Id, token.UserId);

            await _store.RevokeAsync(token, "Token expired", ipAddress, cancellationToken);

            return RefreshTokenValidationResult.Failure(
                "Refresh token has expired.",
                RefreshTokenValidationError.Expired);
        }

        // Token reuse detection - if this token was already used to generate a new one
        if (token.ReplacedByTokenId.HasValue)
        {
            _logger.LogWarning(
                "Refresh token reuse detected for token {TokenId}, user {UserId}. Potential token theft.",
                token.Id, token.UserId);

            if (_options.RevokeOnReuse)
            {
                // Revoke the entire token family
                await RevokeTokenFamilyAsync(token, ipAddress, cancellationToken);
            }

            return RefreshTokenValidationResult.Failure(
                "Refresh token has been reused. For security, all related tokens have been revoked.",
                RefreshTokenValidationError.Reused);
        }

        // Mark current token as used
        token.MarkAsUsed(ipAddress);

        string newRefreshToken;

        if (_options.EnableRotation)
        {
            // Generate new refresh token
            newRefreshToken = await GenerateRotatedTokenAsync(token, ipAddress, cancellationToken);
        }
        else
        {
            newRefreshToken = refreshToken;
            await _store.StoreAsync(token, cancellationToken);
        }

        _logger.LogInformation(
            "Refresh token validated and rotated for user {UserId}",
            token.UserId);

        return RefreshTokenValidationResult.Success(
            token.UserId,
            newRefreshToken,
            token.TenantId);
    }

    /// <inheritdoc/>
    public async Task RevokeAsync(
        string token,
        string reason,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _store.GetByTokenAsync(token, cancellationToken);

        if (refreshToken != null)
        {
            await _store.RevokeAsync(refreshToken, reason, ipAddress, cancellationToken);
            _logger.LogInformation(
                "Revoked refresh token {TokenId} for user {UserId}: {Reason}",
                refreshToken.Id, refreshToken.UserId, reason);
        }
    }

    /// <inheritdoc/>
    public async Task RevokeAllAsync(string userId, string reason, CancellationToken cancellationToken = default)
    {
        await _store.RevokeAllByUserIdAsync(userId, reason, cancellationToken);
        _logger.LogInformation("Revoked all refresh tokens for user {UserId}: {Reason}", userId, reason);
    }

    /// <inheritdoc/>
    public async Task RevokeAllExceptAsync(
        string userId,
        string currentToken,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _store.GetByUserIdAsync(userId, cancellationToken);

        foreach (var token in tokens.Where(t => t.Token != currentToken && t.IsActive))
        {
            await _store.RevokeAsync(token, reason, null, cancellationToken);
        }

        _logger.LogInformation(
            "Revoked all refresh tokens except current for user {UserId}: {Reason}",
            userId, reason);
    }

    /// <inheritdoc/>
    public async Task<int> CleanupAsync(CancellationToken cancellationToken = default)
    {
        var expiredCount = await _store.RemoveExpiredAsync(cancellationToken);
        var revokedCount = await _store.RemoveRevokedAsync(cancellationToken);

        _logger.LogInformation(
            "Cleanup completed: removed {ExpiredCount} expired and {RevokedCount} revoked tokens",
            expiredCount, revokedCount);

        return expiredCount + revokedCount;
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    /// <returns>A secure random token string.</returns>
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Generates a rotated refresh token.
    /// </summary>
    /// <param name="currentToken">The current refresh token.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new refresh token string.</returns>
    private async Task<string> GenerateRotatedTokenAsync(
        RefreshToken currentToken,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var expiresUtc = DateTime.UtcNow.AddDays(_options.ExpirationDays);
        var newToken = GenerateSecureToken();

        var refreshToken = RefreshToken.Create(
            currentToken.UserId,
            expiresUtc,
            ipAddress,
            currentToken.TenantId,
            currentToken.DeviceId,
            currentToken.UserAgent);

        refreshToken.Token = newToken;
        refreshToken.ReplacesTokenId = currentToken.Id;

        // Link the old token to this new one
        currentToken.ReplacedByTokenId = refreshToken.Id;

        await _store.StoreAsync(currentToken, cancellationToken);
        await _store.StoreAsync(refreshToken, cancellationToken);

        return newToken;
    }

    /// <summary>
    /// Revokes all tokens in the token family (for reuse detection).
    /// </summary>
    /// <param name="token">The token that triggered the reuse detection.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private async Task RevokeTokenFamilyAsync(
        RefreshToken token,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var tokensToRevoke = new List<RefreshToken> { token };

        // Find all tokens that this token replaces (ancestors)
        var currentToken = token;
        while (currentToken.ReplacesTokenId.HasValue)
        {
            var parentToken = await _store.GetByIdAsync(currentToken.ReplacesTokenId.Value, cancellationToken);
            if (parentToken != null)
            {
                tokensToRevoke.Add(parentToken);
                currentToken = parentToken;
            }
            else
            {
                break;
            }
        }

        // Find all tokens that replace this token (descendants)
        currentToken = token;
        while (currentToken.ReplacedByTokenId.HasValue)
        {
            var childToken = await _store.GetByIdAsync(currentToken.ReplacedByTokenId.Value, cancellationToken);
            if (childToken != null)
            {
                tokensToRevoke.Add(childToken);
                currentToken = childToken;
            }
            else
            {
                break;
            }
        }

        // Revoke all tokens in the family
        foreach (var refreshToken in tokensToRevoke)
        {
            if (!refreshToken.IsRevoked)
            {
                await _store.RevokeAsync(
                    refreshToken,
                    "Token family revoked due to reuse detection",
                    ipAddress,
                    cancellationToken);
            }
        }

        _logger.LogWarning(
            "Revoked token family of {Count} tokens for user {UserId} due to reuse detection",
            tokensToRevoke.Count, token.UserId);
    }
}
