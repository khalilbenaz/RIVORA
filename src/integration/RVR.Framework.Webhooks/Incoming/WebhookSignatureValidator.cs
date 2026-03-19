using System.Security.Cryptography;
using System.Text;

namespace RVR.Framework.Webhooks.Incoming;

/// <summary>
/// Validates incoming webhook signatures using HMAC algorithms with timing-safe comparison.
/// </summary>
public static class IncomingWebhookSignatureValidator
{
    /// <summary>
    /// Validates an HMAC-SHA256 signature against the payload and secret.
    /// Supports both raw hex and <c>sha256=hex</c> prefix format.
    /// </summary>
    public static bool ValidateHmacSha256(string payload, string secret, string signature)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature))
            return false;

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
    /// Validates an HMAC-SHA1 signature against the payload and secret.
    /// Supports both raw hex and <c>sha1=hex</c> prefix format (GitHub-style).
    /// </summary>
    public static bool ValidateHmacSha1(string payload, string secret, string signature)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature))
            return false;

        var signatureHex = signature.StartsWith("sha1=", StringComparison.OrdinalIgnoreCase)
            ? signature["sha1=".Length..]
            : signature;

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hashBytes = HMACSHA1.HashData(keyBytes, payloadBytes);
        var computedHex = Convert.ToHexStringLower(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(signatureHex.ToLowerInvariant()));
    }

    /// <summary>
    /// Dispatches signature validation to the appropriate algorithm.
    /// </summary>
    /// <param name="payload">The raw webhook payload.</param>
    /// <param name="secret">The shared secret key.</param>
    /// <param name="signature">The signature to validate.</param>
    /// <param name="algorithm">The algorithm name: "hmac-sha256" or "hmac-sha1".</param>
    /// <returns>True if the signature is valid; false otherwise.</returns>
    public static bool Validate(string payload, string secret, string signature, string algorithm)
    {
        return algorithm.ToLowerInvariant() switch
        {
            "hmac-sha256" => ValidateHmacSha256(payload, secret, signature),
            "hmac-sha1" => ValidateHmacSha1(payload, secret, signature),
            _ => false
        };
    }
}
