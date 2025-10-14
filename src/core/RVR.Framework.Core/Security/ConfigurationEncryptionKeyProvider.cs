using Microsoft.Extensions.Configuration;

namespace RVR.Framework.Core.Security;

/// <summary>
/// Reads the AES-256 encryption key from <see cref="IConfiguration"/> at the path
/// <c>Security:EncryptionKey</c>.
/// </summary>
public class ConfigurationEncryptionKeyProvider : IEncryptionKeyProvider
{
    private const string ConfigurationPath = "Security:EncryptionKey";
    private readonly IConfiguration _configuration;

    public ConfigurationEncryptionKeyProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public string GetEncryptionKey()
    {
        var key = _configuration[ConfigurationPath];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                $"Encryption key not found at configuration path '{ConfigurationPath}'. " +
                "Provide a 32-character key for AES-256 encryption via configuration (e.g., appsettings.json, " +
                "Azure Key Vault, or user-secrets). Do NOT use a hardcoded default key in production.");
        }

        return key;
    }
}
