namespace KBA.Framework.Security.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Interfaces;
using Microsoft.Extensions.Logging;
using OtpNet;
using QRCoder;

/// <summary>
/// Service for TOTP (Time-based One-Time Password) operations.
/// Compatible with Google Authenticator, Authy, and other TOTP apps.
/// </summary>
public class TotpService : ITotpService
{
    private readonly IBackupCodeStore _backupCodeStore;
    private readonly ILogger<TotpService> _logger;
    private readonly TotpOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TotpService"/> class.
    /// </summary>
    /// <param name="backupCodeStore">The backup code store.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The TOTP options.</param>
    public TotpService(
        IBackupCodeStore backupCodeStore,
        ILogger<TotpService> logger,
        TotpOptions? options = null)
    {
        _backupCodeStore = backupCodeStore ?? throw new ArgumentNullException(nameof(backupCodeStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new TotpOptions();
    }

    /// <inheritdoc/>
    public Task<string> GenerateSecretAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        // Generate a 20-byte random secret (160 bits)
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var secretBase32 = Base32Encoding.ToString(secretKey);

        _logger.LogInformation("Generated new TOTP secret for user {UserId}", userId);

        return Task.FromResult(secretBase32);
    }

    /// <inheritdoc/>
    public string GenerateQrCodeUri(string secret, string accountName, string issuer)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("Secret cannot be empty.", nameof(secret));
        }

        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new ArgumentException("Account name cannot be empty.", nameof(accountName));
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Issuer cannot be empty.", nameof(issuer));
        }

        // URL encode the issuer and account name
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedAccountName = Uri.EscapeDataString(accountName);

        // Generate the OTP Auth URI
        // Format: otpauth://totp/ISSUER:ACCOUNT?secret=SECRET&issuer=ISSUER&algorithm=SHA1&digits=6&period=30
        var uri = $"otpauth://totp/{encodedIssuer}:{encodedAccountName}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits={_options.Digits}&period={_options.Period}";

        return uri;
    }

    /// <inheritdoc/>
    public byte[] GenerateQrCodePng(string qrCodeUri)
    {
        if (string.IsNullOrWhiteSpace(qrCodeUri))
        {
            throw new ArgumentException("QR code URI cannot be empty.", nameof(qrCodeUri));
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        // Generate PNG with 20 pixels per module for better readability
        var pngBytes = qrCode.GetGraphic(20);

        _logger.LogDebug("Generated QR code PNG ({Length} bytes)", pngBytes.Length);

        return pngBytes;
    }

    /// <inheritdoc/>
    public Task<bool> ValidateCodeAsync(string secret, string code, CancellationToken cancellationToken = default)
    {
        return ValidateCodeWithToleranceAsync(secret, code, 0, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> ValidateCodeWithToleranceAsync(
        string secret,
        string code,
        int timeWindow = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("Secret cannot be empty.", nameof(secret));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Task.FromResult(false);
        }

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes, _options.Period, OtpHashMode.Sha1, _options.Digits);

            // Check current time step
            if (totp.VerifyTotp(code, out _, new VerificationWindow(timeWindow, timeWindow)))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating TOTP code");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public async Task<BackupCodesResult> GenerateBackupCodesAsync(
        string userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (count <= 0 || count > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 1 and 100.");
        }

        var codes = new List<string>();
        var hashedCodes = new List<string>();

        // Generate secure random backup codes
        for (var i = 0; i < count; i++)
        {
            var code = GenerateSecureBackupCode();
            var hashedCode = HashBackupCode(code);

            codes.Add(code);
            hashedCodes.Add(hashedCode);
        }

        var result = new BackupCodesResult
        {
            UserId = userId,
            Codes = codes.ToArray(),
            HashedCodes = hashedCodes.ToArray(),
            GeneratedAt = DateTime.UtcNow
        };

        // Store the hashed codes
        await _backupCodeStore.StoreAsync(userId, hashedCodes, cancellationToken);

        _logger.LogInformation(
            "Generated {Count} backup codes for user {UserId}",
            count, userId);

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateBackupCodeAsync(
        string userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var hashedCode = HashBackupCode(code);
        var isValid = await _backupCodeStore.ValidateAsync(userId, hashedCode, cancellationToken);

        if (isValid)
        {
            // Revoke the used code
            await _backupCodeStore.RevokeAsync(userId, hashedCode, cancellationToken);

            _logger.LogInformation(
                "Backup code validated and revoked for user {UserId}",
                userId);
        }

        return isValid;
    }

    /// <inheritdoc/>
    public Task RevokeBackupCodeAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        var hashedCode = HashBackupCode(code);
        return _backupCodeStore.RevokeAsync(userId, hashedCode, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<int> GetRemainingBackupCodesCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        return _backupCodeStore.GetRemainingCountAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Generates a secure backup code.
    /// Format: XXXXX-XXXXX (10 characters, easy to read)
    /// </summary>
    /// <returns>A secure backup code.</returns>
    private static string GenerateSecureBackupCode()
    {
        // Use uppercase letters and digits, excluding ambiguous characters (0, O, 1, I, l)
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[10];

        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[10];
        rng.GetBytes(randomBytes);

        for (var i = 0; i < 10; i++)
        {
            code[i] = alphabet[randomBytes[i] % alphabet.Length];
        }

        // Format as XXXXX-XXXXX
        return new string(code.Take(5).ToArray()) + "-" + new string(code.Skip(5).ToArray());
    }

    /// <summary>
    /// Hashes a backup code for secure storage.
    /// </summary>
    /// <param name="code">The backup code to hash.</param>
    /// <returns>The hashed code.</returns>
    private static string HashBackupCode(string code)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code.ToUpperInvariant()));
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// Configuration options for TOTP service.
/// </summary>
public class TotpOptions
{
    /// <summary>
    /// Gets or sets the number of digits in the TOTP code.
    /// Default is 6 (standard for most authenticator apps).
    /// </summary>
    public int Digits { get; set; } = 6;

    /// <summary>
    /// Gets or sets the time step period in seconds.
    /// Default is 30 seconds (standard for TOTP).
    /// </summary>
    public int Period { get; set; } = 30;

    /// <summary>
    /// Gets or sets the issuer name for QR code generation.
    /// </summary>
    public string Issuer { get; set; } = "KBA Framework";
}

/// <summary>
/// Defines the contract for backup code storage operations.
/// </summary>
public interface IBackupCodeStore
{
    /// <summary>
    /// Stores backup codes for a user.
    /// </summary>
    Task StoreAsync(string userId, IEnumerable<string> hashedCodes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a backup code.
    /// </summary>
    Task<bool> ValidateAsync(string userId, string hashedCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a backup code.
    /// </summary>
    Task RevokeAsync(string userId, string hashedCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the remaining backup codes count.
    /// </summary>
    Task<int> GetRemainingCountAsync(string userId, CancellationToken cancellationToken = default);
}
