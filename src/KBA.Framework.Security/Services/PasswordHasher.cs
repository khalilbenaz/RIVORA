namespace KBA.Framework.Security.Services;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for password hashing operations using BCrypt.
/// Provides secure password hashing with automatic salting and configurable work factor.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;
    private const int WorkFactor = 12; // OWASP recommended minimum for BCrypt

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHasher"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            // Use BCrypt Enhanced version with work factor 12
            // This provides ~250ms hash time, making brute force infeasible while maintaining good UX
            var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);

            _logger.LogDebug("Password hashed successfully using BCrypt with work factor {WorkFactor}", WorkFactor);

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
