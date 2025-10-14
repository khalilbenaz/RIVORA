namespace RVR.Framework.Alerting.Channels;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RVR.Framework.Alerting.Interfaces;
using RVR.Framework.Alerting.Models;

/// <summary>
/// Alert channel that sends notifications via Slack webhook.
/// </summary>
public class SlackAlertChannel : IAlertChannel
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private readonly ILogger<SlackAlertChannel>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlackAlertChannel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="webhookUrl">The Slack webhook URL.</param>
    /// <param name="logger">The logger.</param>
    public SlackAlertChannel(
        HttpClient httpClient,
        string webhookUrl,
        ILogger<SlackAlertChannel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var emoji = alert.Severity switch
        {
            AlertSeverity.Info => ":information_source:",
            AlertSeverity.Warning => ":warning:",
            AlertSeverity.Error => ":x:",
            AlertSeverity.Critical => ":rotating_light:",
            _ => ":bell:"
        };

        var metadataText = alert.Metadata.Count > 0
            ? "\n" + string.Join("\n", alert.Metadata.Select(kv => $"*{kv.Key}:* {kv.Value}"))
            : string.Empty;

        var payload = new
        {
            text = $"{emoji} *[{alert.Severity}] {alert.Title}*\n{alert.Message}\n_Source: {alert.Source} | {alert.Timestamp:u}_{metadataText}"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_webhookUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger?.LogDebug("Slack alert sent: {Title}", alert.Title);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send Slack alert: {Title}", alert.Title);
            throw;
        }
    }
}
