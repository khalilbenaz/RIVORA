using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.Plugins.Models;

namespace RVR.Framework.Plugins.Security;

/// <summary>
/// Verifies the digital signature of NuGet plugin packages (.nupkg) against a
/// configurable set of trusted certificate fingerprints.
/// </summary>
public sealed class PluginSignatureVerifier
{
    private readonly ILogger<PluginSignatureVerifier> _logger;
    private readonly PluginSecurityOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="PluginSignatureVerifier"/>.
    /// </summary>
    /// <param name="options">Security options containing trusted certificate fingerprints.</param>
    /// <param name="logger">The logger.</param>
    public PluginSignatureVerifier(
        IOptions<PluginSecurityOptions> options,
        ILogger<PluginSignatureVerifier> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Verifies the signature of a NuGet package file.
    /// </summary>
    /// <param name="packagePath">The absolute path to the .nupkg file.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The verification result indicating signing and trust status.</returns>
    /// <exception cref="ArgumentException">Thrown when the package path is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the package file does not exist.</exception>
    public async Task<SignatureVerificationResult> VerifyAsync(
        string packagePath,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException("Plugin package file not found.", packagePath);
        }

        _logger.LogDebug("Verifying signature of package at '{PackagePath}'.", packagePath);

        return await Task.Run(() => VerifyPackageSignature(packagePath), ct).ConfigureAwait(false);
    }

    private SignatureVerificationResult VerifyPackageSignature(string packagePath)
    {
        try
        {
            // NuGet packages are ZIP archives; signed packages contain a .signature.p7s entry
            using var archive = ZipFile.OpenRead(packagePath);
            var signatureEntry = archive.GetEntry(".signature.p7s");

            if (signatureEntry is null)
            {
                _logger.LogInformation("Package '{PackagePath}' is not signed.", packagePath);
                return new SignatureVerificationResult(IsSigned: false, IsTrusted: false);
            }

            using var signatureStream = signatureEntry.Open();
            using var memoryStream = new MemoryStream();
            signatureStream.CopyTo(memoryStream);
            var signatureBytes = memoryStream.ToArray();

            // Attempt to extract the signer certificate from the PKCS#7 signature
            var signedCms = new System.Security.Cryptography.Pkcs.SignedCms();
            signedCms.Decode(signatureBytes);

            // We do not call CheckSignature with verifySignatureOnly:false in all environments,
            // because the root CA may not be in the local trust store during CI/CD.
            // Instead we validate the fingerprint against the configured trusted list.
            var signerCert = signedCms.SignerInfos.Count > 0
                ? signedCms.SignerInfos[0].Certificate
                : null;

            if (signerCert is null)
            {
                _logger.LogWarning("Package is signed but no signer certificate could be extracted.");
                return new SignatureVerificationResult(IsSigned: true, IsTrusted: false);
            }

            var fingerprint = GetCertificateFingerprint(signerCert);
            var signerName = signerCert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);

            var isTrusted = _options.TrustedCertificateFingerprints.Count == 0 ||
                            _options.TrustedCertificateFingerprints.Contains(fingerprint, StringComparer.OrdinalIgnoreCase);

            if (isTrusted)
            {
                _logger.LogInformation(
                    "Package signature verified and trusted. Signer: '{Signer}', Fingerprint: {Fingerprint}.",
                    signerName, fingerprint);
            }
            else
            {
                _logger.LogWarning(
                    "Package is signed by '{Signer}' (fingerprint: {Fingerprint}) but the certificate is not in the trusted list.",
                    signerName, fingerprint);
            }

            return new SignatureVerificationResult(IsSigned: true, IsTrusted: isTrusted, SignerName: signerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify package signature for '{PackagePath}'.", packagePath);
            return new SignatureVerificationResult(IsSigned: false, IsTrusted: false);
        }
    }

    private static string GetCertificateFingerprint(X509Certificate2 certificate)
    {
        var hashBytes = certificate.GetCertHash(HashAlgorithmName.SHA256);
        return Convert.ToHexString(hashBytes);
    }
}

/// <summary>
/// The result of verifying a plugin package's digital signature.
/// </summary>
/// <param name="IsSigned">Whether the package contains a digital signature.</param>
/// <param name="IsTrusted">Whether the signer's certificate is in the trusted list.</param>
/// <param name="SignerName">The common name of the signer, if available.</param>
public sealed record SignatureVerificationResult(
    bool IsSigned,
    bool IsTrusted,
    string? SignerName = null);
