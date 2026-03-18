using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.SMS.Configuration;
using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS.Providers;

/// <summary>
/// SMS provider implementation using the Vonage (formerly Nexmo) REST API.
/// </summary>
public sealed class VonageSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly VonageOptions _options;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<VonageSmsProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="VonageSmsProvider"/>.
    /// </summary>
    public VonageSmsProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SmsOptions> smsOptions,
        ILogger<VonageSmsProvider> logger)
    {
        _smsOptions = smsOptions.Value;
        _options = _smsOptions.Vonage ?? throw new InvalidOperationException("Vonage SMS options are not configured.");
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("RVR.SMS.Vonage");
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/'));
    }

    /// <inheritdoc />
    public SmsProvider ProviderType => SmsProvider.Vonage;

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        var from = message.From ?? _options.FromName ?? _smsOptions.DefaultFrom;
        if (string.IsNullOrWhiteSpace(from))
        {
            return new SmsResult(false, Error: "No sender configured for Vonage.", Provider: ProviderType);
        }

        var payload = new
        {
            api_key = _options.ApiKey,
            api_secret = _options.ApiSecret,
            to = message.To,
            from,
            text = message.Body
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        try
        {
            _logger.LogDebug("Sending SMS via Vonage to {To}", message.To);
            var response = await _httpClient.PostAsync("/sms/json", jsonContent, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var messages = doc.RootElement.GetProperty("messages");
                if (messages.GetArrayLength() > 0)
                {
                    var first = messages[0];
                    var status = first.GetProperty("status").GetString();

                    if (status == "0")
                    {
                        var msgId = first.GetProperty("message-id").GetString();
                        _logger.LogInformation("SMS sent via Vonage. MessageId={MessageId}, To={To}", msgId, message.To);
                        return new SmsResult(true, MessageId: msgId, Provider: ProviderType);
                    }

                    var errorText = first.TryGetProperty("error-text", out var errEl) ? errEl.GetString() : "Unknown error";
                    _logger.LogWarning("Vonage API returned error status {Status}: {Error}", status, errorText);
                    return new SmsResult(false, Error: $"Vonage error (status {status}): {errorText}", Provider: ProviderType);
                }
            }

            _logger.LogWarning("Vonage API returned {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
            return new SmsResult(false, Error: $"Vonage API error ({(int)response.StatusCode}): {responseBody}", Provider: ProviderType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send SMS via Vonage to {To}", message.To);
            return new SmsResult(false, Error: ex.Message, Provider: ProviderType);
        }
    }

    /// <inheritdoc />
    public Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default)
    {
        // Vonage uses webhooks for delivery receipts rather than a polling API.
        // Status queries are not natively supported; return Unknown.
        _logger.LogDebug("Vonage does not support status polling. Use delivery receipt webhooks instead. MessageId={MessageId}", messageId);
        return Task.FromResult(new SmsStatus(messageId, SmsDeliveryStatus.Unknown));
    }
}
