using System.Net;
using System.Net.Sockets;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// Validates webhook callback URLs to prevent SSRF attacks by blocking
/// private networks, loopback addresses, and non-allowed schemes.
/// </summary>
public static class CallbackUrlValidator
{
    /// <summary>
    /// Validates a callback URL against the provided webhook options.
    /// Throws <see cref="ArgumentException"/> if the URL is invalid or targets a blocked network.
    /// </summary>
    /// <param name="callbackUrl">The URL to validate.</param>
    /// <param name="options">The webhook options containing validation rules.</param>
    public static void Validate(string callbackUrl, WebhookOptions options)
    {
        if (!Uri.TryCreate(callbackUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"The callback URL '{callbackUrl}' is not a valid absolute URI.", nameof(callbackUrl));
        }

        // Validate scheme
        if (options.AllowedSchemes is { Length: > 0 })
        {
            var schemeAllowed = options.AllowedSchemes.Any(s =>
                string.Equals(s, uri.Scheme, StringComparison.OrdinalIgnoreCase));

            if (!schemeAllowed)
            {
                throw new ArgumentException(
                    $"The callback URL scheme '{uri.Scheme}' is not allowed. Allowed schemes: {string.Join(", ", options.AllowedSchemes)}.",
                    nameof(callbackUrl));
            }
        }

        // Validate against private/loopback networks
        if (options.BlockPrivateNetworks)
        {
            ValidateNotPrivateNetwork(uri, callbackUrl);
        }
    }

    private static void ValidateNotPrivateNetwork(Uri uri, string callbackUrl)
    {
        var host = uri.Host;

        // Block localhost variants
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The callback URL '{callbackUrl}' targets localhost, which is blocked.",
                nameof(callbackUrl));
        }

        // Try to parse as IP address directly
        if (IPAddress.TryParse(host, out var ipAddress))
        {
            if (IsBlockedAddress(ipAddress))
            {
                throw new ArgumentException(
                    $"The callback URL '{callbackUrl}' targets a private or loopback network address, which is blocked.",
                    nameof(callbackUrl));
            }
        }
        else
        {
            // Resolve DNS and check all addresses
            try
            {
                var addresses = Dns.GetHostAddresses(host);
                foreach (var address in addresses)
                {
                    if (IsBlockedAddress(address))
                    {
                        throw new ArgumentException(
                            $"The callback URL '{callbackUrl}' resolves to a private or loopback network address, which is blocked.",
                            nameof(callbackUrl));
                    }
                }
            }
            catch (SocketException)
            {
                throw new ArgumentException(
                    $"The callback URL host '{host}' could not be resolved.",
                    nameof(callbackUrl));
            }
        }
    }

    private static bool IsBlockedAddress(IPAddress address)
    {
        // Loopback (127.0.0.0/8, ::1)
        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            // 0.0.0.0
            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0)
                return true;

            // 169.254.0.0/16 (link-local)
            if (bytes[0] == 169 && bytes[1] == 254)
                return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // IPv6 loopback (::1) is already handled by IPAddress.IsLoopback
            // Block link-local (fe80::/10)
            if (address.IsIPv6LinkLocal)
                return true;

            // Block unique local addresses (fc00::/7)
            var bytes = address.GetAddressBytes();
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;
        }

        return false;
    }
}
