using System.Security.Cryptography;
using System.Text;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// Validates incoming webhook signatures using HMAC-SHA256.
/// </summary>
public static class WebhookSignatureValidator
{
    /// <summary>
    /// Validates that the given signature matches the HMAC-SHA256 hash of the payload using the shared secret.
    /// </summary>
    /// <param name="payload">The raw webhook payload.</param>
    /// <param name="signature">The signature to validate (hex-encoded, optionally prefixed with "sha256=").</param>
    /// <param name="secret">The shared secret key.</param>
    /// <returns>True if the signature is valid; false otherwise.</returns>
    public static bool ValidateHmacSha256(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
            return false;

        // Strip common prefix
        var signatureHex = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? signature["sha256=".Length..]
            : signature;

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        var hashBytes = HMACSHA256.HashData(keyBytes, payloadBytes);
        var computedHex = Convert.ToHexStringLower(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(signatureHex.ToLowerInvariant()));
    }

    /// <summary>
    /// Validates that the given signature matches the HMAC-SHA256 hash of the payload bytes using the shared secret.
    /// </summary>
    /// <param name="payload">The raw webhook payload bytes.</param>
    /// <param name="signature">The signature to validate (hex-encoded, optionally prefixed with "sha256=").</param>
    /// <param name="secret">The shared secret key.</param>
    /// <returns>True if the signature is valid; false otherwise.</returns>
    public static bool ValidateHmacSha256(byte[] payload, string signature, string secret)
    {
        if (payload.Length == 0 || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
            return false;

        var signatureHex = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? signature["sha256=".Length..]
            : signature;

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var hashBytes = HMACSHA256.HashData(keyBytes, payload);
        var computedHex = Convert.ToHexStringLower(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(signatureHex.ToLowerInvariant()));
    }
}
