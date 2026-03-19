using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.SMS.Configuration;
using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS.Providers;

/// <summary>
/// SMS provider implementation using the Twilio REST API.
/// Sends messages via POST to <c>/2010-04-01/Accounts/{AccountSid}/Messages.json</c>.
/// </summary>
public sealed class TwilioSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly TwilioOptions _options;
    private readonly SmsOptions _smsOptions;
    private readonly ILogger<TwilioSmsProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TwilioSmsProvider"/>.
    /// </summary>
    public TwilioSmsProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SmsOptions> smsOptions,
        ILogger<TwilioSmsProvider> logger)
    {
        _smsOptions = smsOptions.Value;
        _options = _smsOptions.Twilio ?? throw new InvalidOperationException("Twilio SMS options are not configured.");
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("RVR.SMS.Twilio");

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.AccountSid}:{_options.AuthToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/'));
    }

    /// <inheritdoc />
    public SmsProvider ProviderType => SmsProvider.Twilio;

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        var from = message.From ?? _options.FromNumber ?? _smsOptions.DefaultFrom;
        if (string.IsNullOrWhiteSpace(from))
        {
            return new SmsResult(false, Error: "No sender number configured for Twilio.", Provider: ProviderType);
        }

        var requestUri = $"/2010-04-01/Accounts/{_options.AccountSid}/Messages.json";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = message.To,
            ["From"] = from,
            ["Body"] = message.Body
        });

        try
        {
            _logger.LogDebug("Sending SMS via Twilio to {To}", message.To);
            var response = await _httpClient.PostAsync(requestUri, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var sid = doc.RootElement.GetProperty("sid").GetString();
                _logger.LogInformation("SMS sent via Twilio. MessageSid={MessageSid}, To={To}", sid, message.To);
                return new SmsResult(true, MessageId: sid, Provider: ProviderType);
            }

            _logger.LogWarning("Twilio API returned {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
            return new SmsResult(false, Error: $"Twilio API error ({(int)response.StatusCode}): {responseBody}", Provider: ProviderType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send SMS via Twilio to {To}", message.To);
            return new SmsResult(false, Error: ex.Message, Provider: ProviderType);
        }
    }

    /// <inheritdoc />
    public async Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default)
    {
        var requestUri = $"/2010-04-01/Accounts/{_options.AccountSid}/Messages/{messageId}.json";

        try
        {
            var response = await _httpClient.GetAsync(requestUri, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twilio status query failed ({StatusCode}): {Body}", (int)response.StatusCode, responseBody);
                return new SmsStatus(messageId, SmsDeliveryStatus.Unknown);
            }

            using var doc = JsonDocument.Parse(responseBody);
            var status = doc.RootElement.GetProperty("status").GetString();
            var deliveryStatus = MapTwilioStatus(status);

            DateTime? deliveredAt = null;
            if (doc.RootElement.TryGetProperty("date_updated", out var dateEl) && dateEl.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(dateEl.GetString(), out var parsed))
                    deliveredAt = parsed;
            }

            return new SmsStatus(messageId, deliveryStatus, deliveredAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to query SMS status from Twilio for {MessageId}", messageId);
            return new SmsStatus(messageId, SmsDeliveryStatus.Unknown);
        }
    }

    private static SmsDeliveryStatus MapTwilioStatus(string? status) => status switch
    {
        "queued" or "accepted" => SmsDeliveryStatus.Pending,
        "sending" or "sent" => SmsDeliveryStatus.Sent,
        "delivered" => SmsDeliveryStatus.Delivered,
        "failed" or "undelivered" => SmsDeliveryStatus.Failed,
        _ => SmsDeliveryStatus.Unknown
    };
}
