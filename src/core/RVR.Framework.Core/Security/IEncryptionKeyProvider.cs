namespace RVR.Framework.Core.Security;

/// <summary>
/// Provides the encryption key used for AES-256 data-at-rest encryption.
/// Implementations must return a key that is exactly 32 characters (256 bits).
/// </summary>
public interface IEncryptionKeyProvider
{
    /// <summary>
    /// Gets the encryption key. Must be exactly 32 characters long.
    /// </summary>
    /// <returns>The encryption key string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no key is configured.</exception>
    string GetEncryptionKey();
}
