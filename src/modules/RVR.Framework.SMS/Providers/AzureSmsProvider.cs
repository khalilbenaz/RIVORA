using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.SMS.Configuration;
using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS.Providers;

/// <summary>
/// SMS provider implementation using Azure Communication Services REST API.
/// </summary>
public sealed class AzureSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly AzureOptions _options;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<AzureSmsProvider> _logger;

    private const string ApiVersion = "2024-07-01-preview";

    /// <summary>
    /// Initializes a new instance of <see cref="AzureSmsProvider"/>.
    /// </summary>
    public AzureSmsProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SmsOptions> smsOptions,
        ILogger<AzureSmsProvider> logger)
    {
        _smsOptions = smsOptions.Value;
        _options = _smsOptions.Azure ?? throw new InvalidOperationException("Azure SMS options are not configured.");
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("RVR.SMS.Azure");

        var endpoint = ResolveEndpoint();
        _httpClient.BaseAddress = new Uri(endpoint.TrimEnd('/'));
    }

    /// <inheritdoc />
    public SmsProvider ProviderType => SmsProvider.Azure;

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        var from = message.From ?? _options.FromNumber ?? _smsOptions.DefaultFrom;
        if (string.IsNullOrWhiteSpace(from))
        {
            return new SmsResult(false, Error: "No sender number configured for Azure Communication Services.", Provider: ProviderType);
        }

        var payload = new
        {
            from,
            smsRecipients = new[]
            {
                new
                {
                    to = message.To,
                    repeatabilityRequestId = Guid.NewGuid().ToString(),
                    repeatabilityFirstSent = DateTime.UtcNow.ToString("R")
                }
            },
            message = message.Body
        };

        var jsonBody = JsonSerializer.Serialize(payload);
        var requestUri = $"/sms?api-version={ApiVersion}";

        try
        {
            _logger.LogDebug("Sending SMS via Azure Communication Services to {To}", message.To);

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            SignRequest(request, jsonBody);

            var response = await _httpClient.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                string? msgId = null;

                if (doc.RootElement.TryGetProperty("value", out var valueEl) && valueEl.GetArrayLength() > 0)
                {
                    var first = valueEl[0];
                    if (first.TryGetProperty("messageId", out var idEl))
                        msgId = idEl.GetString();

                    if (first.TryGetProperty("successful", out var successEl) && !successEl.GetBoolean())
                    {
                        var errorMsg = first.TryGetProperty("errorMessage", out var errEl) ? errEl.GetString() : "Unknown error";
                        return new SmsResult(false, Error: errorMsg, Provider: ProviderType);
                    }
                }

                _logger.LogInformation("SMS sent via Azure. MessageId={MessageId}, To={To}", msgId, message.To);
                return new SmsResult(true, MessageId: msgId, Provider: ProviderType);
            }

            _logger.LogWarning("Azure SMS API returned {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
            return new SmsResult(false, Error: $"Azure SMS API error ({(int)response.StatusCode}): {responseBody}", Provider: ProviderType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send SMS via Azure to {To}", message.To);
            return new SmsResult(false, Error: ex.Message, Provider: ProviderType);
        }
    }

    /// <inheritdoc />
    public Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default)
    {
        // Azure Communication Services uses Event Grid for delivery reports.
        _logger.LogDebug("Azure Communication Services uses Event Grid for delivery reports. MessageId={MessageId}", messageId);
        return Task.FromResult(new SmsStatus(messageId, SmsDeliveryStatus.Unknown));
    }

    /// <summary>
    /// Resolves the Azure Communication Services endpoint from the connection string or explicit endpoint.
    /// </summary>
    private string ResolveEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(_options.Endpoint))
            return _options.Endpoint;

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            var parts = _options.ConnectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("endpoint=", StringComparison.OrdinalIgnoreCase))
                    return part["endpoint=".Length..];
            }
        }

        throw new InvalidOperationException("Azure Communication Services endpoint is not configured.");
    }

    /// <summary>
    /// Resolves the access key from configuration or connection string.
    /// </summary>
    private string ResolveAccessKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.AccessKey))
            return _options.AccessKey;

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            var parts = _options.ConnectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("accesskey=", StringComparison.OrdinalIgnoreCase))
                    return part["accesskey=".Length..];
            }
        }

        throw new InvalidOperationException("Azure Communication Services access key is not configured.");
    }

    /// <summary>
    /// Signs the request using HMAC-SHA256 for Azure Communication Services authentication.
    /// </summary>
    private void SignRequest(HttpRequestMessage request, string body)
    {
        var accessKey = ResolveAccessKey();
        var utcNow = DateTimeOffset.UtcNow.ToString("R");
        var contentHash = ComputeContentHash(body);
        var host = request.RequestUri?.Host ?? _httpClient.BaseAddress?.Host ?? string.Empty;
        var pathAndQuery = request.RequestUri?.PathAndQuery ?? "/sms";

        var stringToSign = $"POST\n{pathAndQuery}\n{utcNow};{host};{contentHash}";
        var keyBytes = Convert.FromBase64String(accessKey);
        var signatureBytes = HMACSHA256.HashData(keyBytes, Encoding.UTF8.GetBytes(stringToSign));
        var signature = Convert.ToBase64String(signatureBytes);

        request.Headers.Add("x-ms-date", utcNow);
        request.Headers.Add("x-ms-content-sha256", contentHash);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "HMAC-SHA256",
            $"SignedHeaders=x-ms-date;host;x-ms-content-sha256&Signature={signature}");
    }

    private static string ComputeContentHash(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hash);
    }
}
