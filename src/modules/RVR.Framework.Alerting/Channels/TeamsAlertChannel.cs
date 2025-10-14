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
/// Alert channel that sends notifications via Microsoft Teams webhook using AdaptiveCard.
/// </summary>
public class TeamsAlertChannel : IAlertChannel
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private readonly ILogger<TeamsAlertChannel>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamsAlertChannel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="webhookUrl">The Teams webhook URL.</param>
    /// <param name="logger">The logger.</param>
    public TeamsAlertChannel(
        HttpClient httpClient,
        string webhookUrl,
        ILogger<TeamsAlertChannel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var color = alert.Severity switch
        {
            AlertSeverity.Info => "good",
            AlertSeverity.Warning => "warning",
            AlertSeverity.Error => "attention",
            AlertSeverity.Critical => "attention",
            _ => "default"
        };

        var facts = new List<object>
        {
            new { title = "Severity", value = alert.Severity.ToString() },
            new { title = "Source", value = alert.Source },
            new { title = "Time", value = alert.Timestamp.ToString("u") }
        };

        foreach (var kv in alert.Metadata)
        {
            facts.Add(new { title = kv.Key, value = kv.Value });
        }

        var adaptiveCard = new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = new
                    {
                        type = "AdaptiveCard",
                        version = "1.4",
                        body = new object[]
                        {
                            new
                            {
                                type = "TextBlock",
                                text = alert.Title,
                                weight = "Bolder",
                                size = "Medium",
                                color
                            },
                            new
                            {
                                type = "TextBlock",
                                text = alert.Message,
                                wrap = true
                            },
                            new
                            {
                                type = "FactSet",
                                facts
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(adaptiveCard);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_webhookUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger?.LogDebug("Teams alert sent: {Title}", alert.Title);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send Teams alert: {Title}", alert.Title);
            throw;
        }
    }
}
