using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.SMS.Configuration;
using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS.Providers;

/// <summary>
/// SMS provider implementation using the OVH SMS REST API.
/// Authenticates using OVH application key, secret, and consumer key with request signing.
/// </summary>
public sealed class OvhSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly OvhOptions _options;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<OvhSmsProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OvhSmsProvider"/>.
    /// </summary>
    public OvhSmsProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SmsOptions> smsOptions,
        ILogger<OvhSmsProvider> logger)
    {
        _smsOptions = smsOptions.Value;
        _options = _smsOptions.OVH ?? throw new InvalidOperationException("OVH SMS options are not configured.");
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("RVR.SMS.OVH");
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/'));
    }

    /// <inheritdoc />
    public SmsProvider ProviderType => SmsProvider.OVH;

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        var sender = message.From ?? _options.SenderName ?? _smsOptions.DefaultFrom;

        var payload = new Dictionary<string, object>
        {
            ["message"] = message.Body,
            ["receivers"] = new[] { message.To },
            ["noStopClause"] = true
        };

        if (!string.IsNullOrWhiteSpace(sender))
            payload["sender"] = sender;

        var jsonBody = JsonSerializer.Serialize(payload);
        var requestUri = $"/sms/{_options.ServiceName}/jobs";
        var method = "POST";

        try
        {
            _logger.LogDebug("Sending SMS via OVH to {To}", message.To);

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            SignRequest(request, method, requestUri, jsonBody);

            var response = await _httpClient.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                string? msgId = null;
                if (doc.RootElement.TryGetProperty("ids", out var idsEl) && idsEl.GetArrayLength() > 0)
                {
                    msgId = idsEl[0].ToString();
                }

                _logger.LogInformation("SMS sent via OVH. MessageId={MessageId}, To={To}", msgId, message.To);
                return new SmsResult(true, MessageId: msgId, Provider: ProviderType);
            }

            _logger.LogWarning("OVH API returned {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
            return new SmsResult(false, Error: $"OVH API error ({(int)response.StatusCode}): {responseBody}", Provider: ProviderType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send SMS via OVH to {To}", message.To);
            return new SmsResult(false, Error: ex.Message, Provider: ProviderType);
        }
    }

    /// <inheritdoc />
    public async Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default)
    {
        var requestUri = $"/sms/{_options.ServiceName}/jobs/{messageId}";
        var method = "GET";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            SignRequest(request, method, requestUri, string.Empty);

            var response = await _httpClient.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OVH status query failed ({StatusCode}): {Body}", (int)response.StatusCode, responseBody);
                return new SmsStatus(messageId, SmsDeliveryStatus.Unknown);
            }

            using var doc = JsonDocument.Parse(responseBody);
            var deliveryStatus = SmsDeliveryStatus.Unknown;

            if (doc.RootElement.TryGetProperty("deliveryReceipt", out var receiptEl))
            {
                var receipt = receiptEl.GetInt32();
                deliveryStatus = receipt switch
                {
                    0 => SmsDeliveryStatus.Pending,
                    1 => SmsDeliveryStatus.Delivered,
                    2 or 3 => SmsDeliveryStatus.Failed,
                    _ => SmsDeliveryStatus.Unknown
                };
            }

            return new SmsStatus(messageId, deliveryStatus);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to query SMS status from OVH for {MessageId}", messageId);
            return new SmsStatus(messageId, SmsDeliveryStatus.Unknown);
        }
    }

    /// <summary>
    /// Signs an OVH API request using the application secret and consumer key.
    /// </summary>
    private void SignRequest(HttpRequestMessage request, string method, string path, string body)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var fullUrl = _options.BaseUrl.TrimEnd('/') + path;
        var toSign = $"{_options.ApplicationSecret}+{_options.ConsumerKey}+{method}+{fullUrl}+{body}+{timestamp}";

        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(toSign));
        var signature = "$1$" + Convert.ToHexStringLower(hash);

        request.Headers.Add("X-Ovh-Application", _options.ApplicationKey);
        request.Headers.Add("X-Ovh-Timestamp", timestamp);
        request.Headers.Add("X-Ovh-Signature", signature);
        request.Headers.Add("X-Ovh-Consumer", _options.ConsumerKey);
    }
}
