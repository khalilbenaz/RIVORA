namespace KBA.Framework.Security.Services;

/// <summary>
/// Defines the contract for password hashing operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using a secure hashing algorithm.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}
