namespace RVR.Framework.Alerting.Models;

/// <summary>
/// Represents an alert to be sent through alert channels.
/// </summary>
public class Alert
{
    /// <summary>
    /// Gets or sets the severity of the alert.
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Info;

    /// <summary>
    /// Gets or sets the title of the alert.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body of the alert.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source of the alert (e.g., service name, component).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata associated with the alert.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the timestamp when the alert was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Alert severity levels.
/// </summary>
public enum AlertSeverity
{
    /// <summary>Informational alert.</summary>
    Info = 0,

    /// <summary>Warning alert.</summary>
    Warning = 1,

    /// <summary>Error alert.</summary>
    Error = 2,

    /// <summary>Critical alert requiring immediate attention.</summary>
    Critical = 3
}
