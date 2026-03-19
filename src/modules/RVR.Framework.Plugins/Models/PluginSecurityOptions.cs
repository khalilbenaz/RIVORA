namespace RVR.Framework.Plugins.Models;

/// <summary>
/// Configuration options for RIVORA plugin security, including trusted certificate fingerprints
/// used during package signature verification.
/// </summary>
public sealed class PluginSecurityOptions
{
    /// <summary>
    /// Whether to require valid signatures for all installed plugins. Defaults to <c>false</c>.
    /// </summary>
    public bool RequireSignedPackages { get; set; }

    /// <summary>
    /// A list of SHA-256 certificate fingerprints (hex-encoded) that are trusted for plugin signing.
    /// When empty, all valid signatures are trusted.
    /// </summary>
    public List<string> TrustedCertificateFingerprints { get; set; } = [];
}
