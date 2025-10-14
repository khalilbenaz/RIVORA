namespace RVR.Framework.Security.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for refresh token service operations.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new refresh token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="deviceId">The optional device identifier.</param>
    /// <param name="userAgent">The optional user agent string.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated refresh token.</returns>
    Task<string> GenerateAsync(
        string userId,
        string ipAddress,
        string? tenantId = null,
        string? deviceId = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a refresh token and returns a new access token if valid.
    /// Implements token rotation for enhanced security.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <param name="ipAddress">The IP address for audit purposes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The validation result containing the new tokens if valid.</returns>
    Task<RefreshTokenValidationResult> ValidateAndRotateAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    /// <param name="token">The token to revoke.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="ipAddress">The IP address performing the revocation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAsync(string token, string reason, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAllAsync(string userId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens except the current one (useful for "logout other devices" functionality).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="currentToken">The current token to keep active.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAllExceptAsync(string userId, string currentToken, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired and old revoked tokens.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of tokens cleaned up.</returns>
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a refresh token validation.
/// </summary>
public class RefreshTokenValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the new refresh token (for token rotation).
    /// </summary>
    public string? NewRefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the type of validation failure.
    /// </summary>
    public RefreshTokenValidationError? ErrorType { get; set; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="newRefreshToken">The new refresh token.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <returns>A successful <see cref="RefreshTokenValidationResult"/>.</returns>
    public static RefreshTokenValidationResult Success(string userId, string newRefreshToken, string? tenantId = null)
    {
        return new RefreshTokenValidationResult
        {
            IsValid = true,
            UserId = userId,
            TenantId = tenantId,
            NewRefreshToken = newRefreshToken
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorType">The error type.</param>
    /// <returns>A failed <see cref="RefreshTokenValidationResult"/>.</returns>
    public static RefreshTokenValidationResult Failure(string errorMessage, RefreshTokenValidationError errorType)
    {
        return new RefreshTokenValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            ErrorType = errorType
        };
    }
}

/// <summary>
/// Represents types of refresh token validation errors.
/// </summary>
public enum RefreshTokenValidationError
{
    /// <summary>
    /// The token was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The token has expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The token has been revoked.
    /// </summary>
    Revoked,

    /// <summary>
    /// The token was reused (potential token theft detected).
    /// </summary>
    Reused,

    /// <summary>
    /// The token format is invalid.
    /// </summary>
    InvalidFormat
}
