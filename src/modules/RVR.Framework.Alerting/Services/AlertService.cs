namespace RVR.Framework.Alerting.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.Alerting.Interfaces;
using RVR.Framework.Alerting.Models;

/// <summary>
/// Default implementation of <see cref="IAlertService"/> that dispatches alerts to all registered channels.
/// </summary>
public class AlertService : IAlertService
{
    private readonly IEnumerable<IAlertChannel> _channels;
    private readonly AlertOptions _options;
    private readonly ILogger<AlertService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertService"/> class.
    /// </summary>
    /// <param name="channels">The registered alert channels.</param>
    /// <param name="options">The alert options.</param>
    /// <param name="logger">The logger.</param>
    public AlertService(
        IEnumerable<IAlertChannel> channels,
        IOptions<AlertOptions> options,
        ILogger<AlertService>? logger = null)
    {
        _channels = channels ?? throw new ArgumentNullException(nameof(channels));
        _options = options?.Value ?? new AlertOptions();
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendAlertAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        if (alert.Severity < _options.DefaultSeverityThreshold)
        {
            _logger?.LogDebug(
                "Alert '{Title}' with severity {Severity} is below threshold {Threshold}, skipping",
                alert.Title, alert.Severity, _options.DefaultSeverityThreshold);
            return;
        }

        var channelList = _channels.ToList();

        if (channelList.Count == 0)
        {
            _logger?.LogWarning("No alert channels registered. Alert '{Title}' will not be delivered", alert.Title);
            return;
        }

        _logger?.LogInformation(
            "Dispatching alert '{Title}' (Severity: {Severity}) to {ChannelCount} channel(s)",
            alert.Title, alert.Severity, channelList.Count);

        var tasks = channelList.Select(async channel =>
        {
            try
            {
                await channel.SendAsync(alert, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Failed to send alert '{Title}' through channel {ChannelType}",
                    alert.Title, channel.GetType().Name);
            }
        });

        await Task.WhenAll(tasks);
    }
}
