using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using RVR.Framework.Webhooks.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// Sends webhook HTTP POST requests with proper headers and HMAC-SHA256 signatures.
/// </summary>
public class WebhookSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookSender> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WebhookSender"/>.
    /// </summary>
    public WebhookSender(
        IHttpClientFactory httpClientFactory,
        IOptions<WebhookOptions> options,
        ILogger<WebhookSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sends a webhook event to the specified subscription's callback URL.
    /// </summary>
    /// <param name="subscription">The target subscription.</param>
    /// <param name="webhookEvent">The event to deliver.</param>
    /// <param name="payload">The serialized JSON payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple of (statusCode, success, errorMessage, duration).</returns>
    public async Task<(int StatusCode, bool Success, string? ErrorMessage, TimeSpan Duration)> SendAsync(
        WebhookSubscription subscription,
        WebhookEvent webhookEvent,
        string payload,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("webhooks");
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        var startTime = DateTime.UtcNow;

        try
        {
            using var content = new StringContent(payload, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, subscription.CallbackUrl)
            {
                Content = content
            };

            request.Headers.Add("X-Webhook-Id", webhookEvent.Id);
            request.Headers.Add("X-Webhook-Timestamp", webhookEvent.TimestampUtc.ToString("O"));

            if (!string.IsNullOrEmpty(subscription.Secret))
            {
                var signature = ComputeSignature(payload, subscription.Secret);
                request.Headers.Add(_options.SignatureHeader, $"sha256={signature}");
            }

            if (subscription.Headers is { Length: > 0 })
            {
                foreach (var header in subscription.Headers)
                {
                    var separatorIndex = header.IndexOf(':');
                    if (separatorIndex > 0)
                    {
                        var key = header[..separatorIndex].Trim();
                        var value = header[(separatorIndex + 1)..].Trim();

                        // Block reserved/sensitive headers to prevent injection
                        if (IsReservedHeader(key))
                        {
                            _logger.LogWarning("Skipping reserved webhook header '{HeaderName}'", key);
                            continue;
                        }

                        request.Headers.TryAddWithoutValidation(key, value);
                    }
                }
            }

            var response = await client.SendAsync(request, ct);
            var duration = DateTime.UtcNow - startTime;
            var statusCode = (int)response.StatusCode;
            var success = response.IsSuccessStatusCode;

            if (!success)
            {
                _logger.LogWarning(
                    "Webhook delivery to {CallbackUrl} returned status {StatusCode}",
                    subscription.CallbackUrl, statusCode);
            }

            return (statusCode, success, success ? null : $"HTTP {statusCode}", duration);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Webhook delivery to {CallbackUrl} failed with exception", subscription.CallbackUrl);
            return (0, false, ex.Message, duration);
        }
    }

    private static readonly HashSet<string> ReservedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Host", "Connection", "Content-Length", "Content-Type",
        "Transfer-Encoding", "Cookie", "Set-Cookie", "Proxy-Authorization",
        "X-Webhook-Id", "X-Webhook-Timestamp"
    };

    private static bool IsReservedHeader(string headerName)
        => ReservedHeaders.Contains(headerName);

    private static string ComputeSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexStringLower(hash);
    }
}
