namespace RVR.Framework.Alerting.Channels;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RVR.Framework.Alerting.Interfaces;
using RVR.Framework.Alerting.Models;

/// <summary>
/// Alert channel that logs alerts to <see cref="ILogger"/>.
/// </summary>
public class ConsoleAlertChannel : IAlertChannel
{
    private readonly ILogger<ConsoleAlertChannel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleAlertChannel"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ConsoleAlertChannel(ILogger<ConsoleAlertChannel> logger)
    {
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task SendAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        System.ArgumentNullException.ThrowIfNull(alert);

        var logLevel = alert.Severity switch
        {
            AlertSeverity.Info => LogLevel.Information,
            AlertSeverity.Warning => LogLevel.Warning,
            AlertSeverity.Error => LogLevel.Error,
            AlertSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel,
            "ALERT [{Severity}] {Title} - {Message} (Source: {Source})",
            alert.Severity, alert.Title, alert.Message, alert.Source);

        if (alert.Metadata.Count > 0)
        {
            foreach (var kv in alert.Metadata)
            {
                _logger.Log(logLevel, "  {Key}: {Value}", kv.Key, kv.Value);
            }
        }

        return Task.CompletedTask;
    }
}
