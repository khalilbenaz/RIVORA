namespace RVR.Framework.Core.Security;

/// <summary>
/// Reads the AES-256 encryption key from the environment variable <c>RVR_ENCRYPTION_KEY</c>.
/// </summary>
public class EnvironmentEncryptionKeyProvider : IEncryptionKeyProvider
{
    private const string EnvironmentVariableName = "RVR_ENCRYPTION_KEY";

    /// <inheritdoc />
    public string GetEncryptionKey()
    {
        var key = Environment.GetEnvironmentVariable(EnvironmentVariableName);

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                $"Encryption key not found. Set the '{EnvironmentVariableName}' environment variable " +
                "with a 32-character key for AES-256 encryption. " +
                "Do NOT use a hardcoded default key in production.");
        }

        return key;
    }
}
