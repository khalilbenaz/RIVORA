namespace RVR.Framework.Alerting.Models;

/// <summary>
/// Configuration options for the alerting service.
/// </summary>
public class AlertOptions
{
    /// <summary>
    /// Gets or sets the minimum severity threshold for sending alerts.
    /// Alerts with severity below this threshold will be ignored.
    /// </summary>
    public AlertSeverity DefaultSeverityThreshold { get; set; } = AlertSeverity.Info;

    /// <summary>
    /// Gets or sets the list of channel configurations.
    /// </summary>
    public IList<AlertChannelConfig> Channels { get; set; } = new List<AlertChannelConfig>();
}

/// <summary>
/// Configuration for an individual alert channel.
/// </summary>
public class AlertChannelConfig
{
    /// <summary>
    /// Gets or sets the channel type name.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets channel-specific settings.
    /// </summary>
    public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
}
