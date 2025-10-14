namespace RVR.Framework.Security.Services;

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Service for password hashing operations using BCrypt.
/// Provides secure password hashing with automatic salting and configurable work factor.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;
    private readonly int _workFactor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHasher"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The password hasher options containing the work factor.</param>
    public PasswordHasher(ILogger<PasswordHasher> logger, IOptions<PasswordHasherOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var opts = options?.Value ?? new PasswordHasherOptions();
        _workFactor = opts.WorkFactor;

        if (_workFactor < 4 || _workFactor > 31)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                _workFactor,
                "BCrypt work factor must be between 4 and 31.");
        }
    }

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        }

        try
        {
            // Use BCrypt Enhanced version with configured work factor
            // Default work factor 12 provides ~250ms hash time, making brute force infeasible while maintaining good UX
            var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);

            _logger.LogDebug("Password hashed successfully using BCrypt with work factor {WorkFactor}", _workFactor);

            return hash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            // Use BCrypt Enhanced verification to match the hashing method
            var isValid = BCrypt.Net.BCrypt.EnhancedVerify(password, hash);

            _logger.LogDebug("Password verification result: {IsValid}", isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying password");
            return false;
        }
    }
}
