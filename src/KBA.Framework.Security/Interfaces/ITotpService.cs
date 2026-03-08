namespace KBA.Framework.Security.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for TOTP (Time-based One-Time Password) service operations.
/// </summary>
public interface ITotpService
{
    /// <summary>
    /// Generates a new TOTP secret key for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated secret key.</returns>
    Task<string> GenerateSecretAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a QR code URI for Google Authenticator setup.
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="accountName">The account name (usually email).</param>
    /// <param name="issuer">The issuer name (application name).</param>
    /// <returns>The OTP Auth URI for QR code generation.</returns>
    string GenerateQrCodeUri(string secret, string accountName, string issuer);

    /// <summary>
    /// Generates a QR code as a PNG image.
    /// </summary>
    /// <param name="qrCodeUri">The QR code URI.</param>
    /// <returns>The QR code as a PNG byte array.</returns>
    byte[] GenerateQrCodePng(string qrCodeUri);

    /// <summary>
    /// Validates a TOTP code.
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="code">The TOTP code to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the code is valid.</returns>
    Task<bool> ValidateCodeAsync(string secret, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a TOTP code with a time window tolerance.
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="code">The TOTP code to validate.</param>
    /// <param name="timeWindow">The number of time steps to check before and after current.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the code is valid within the time window.</returns>
    Task<bool> ValidateCodeWithToleranceAsync(
        string secret,
        string code,
        int timeWindow = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates backup codes for account recovery.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The number of codes to generate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The generated backup codes.</returns>
    Task<BackupCodesResult> GenerateBackupCodesAsync(
        string userId,
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a backup code.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The backup code to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the code is valid and unused.</returns>
    Task<bool> ValidateBackupCodeAsync(string userId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a backup code.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="code">The backup code to revoke.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeBackupCodeAsync(string userId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the remaining backup codes count for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of remaining unused backup codes.</returns>
    Task<int> GetRemainingBackupCodesCountAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of backup codes generation.
/// </summary>
public class BackupCodesResult
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated backup codes (plain text - show to user once).
    /// </summary>
    public string[] Codes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the hashed backup codes (for storage).
    /// </summary>
    public string[] HashedCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the date and time when the codes were generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the number of codes.
    /// </summary>
    public int Count => Codes?.Length ?? 0;
}
