namespace RVR.Framework.Alerting.Channels;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RVR.Framework.Alerting.Interfaces;
using RVR.Framework.Alerting.Models;

/// <summary>
/// Defines the contract for an email sender used by the alert channel.
/// This is a local abstraction to avoid coupling to the Email module.
/// </summary>
public interface IAlertEmailSender
{
    /// <summary>
    /// Sends an email with the specified subject, body, and recipient.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body (HTML).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

/// <summary>
/// Alert channel that sends notifications via email, delegating to an <see cref="IAlertEmailSender"/>.
/// </summary>
public class EmailAlertChannel : IAlertChannel
{
    private readonly IAlertEmailSender _emailSender;
    private readonly string _recipientEmail;
    private readonly ILogger<EmailAlertChannel>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAlertChannel"/> class.
    /// </summary>
    /// <param name="emailSender">The email sender.</param>
    /// <param name="recipientEmail">The recipient email address for alerts.</param>
    /// <param name="logger">The logger.</param>
    public EmailAlertChannel(
        IAlertEmailSender emailSender,
        string recipientEmail,
        ILogger<EmailAlertChannel>? logger = null)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _recipientEmail = recipientEmail ?? throw new ArgumentNullException(nameof(recipientEmail));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var subject = $"[{alert.Severity}] {alert.Title}";

        var metadataHtml = alert.Metadata.Count > 0
            ? "<table>" + string.Join("", alert.Metadata.Select(kv => $"<tr><td><strong>{kv.Key}</strong></td><td>{kv.Value}</td></tr>")) + "</table>"
            : string.Empty;

        var body = $@"
<h2>{alert.Title}</h2>
<p><strong>Severity:</strong> {alert.Severity}</p>
<p><strong>Source:</strong> {alert.Source}</p>
<p><strong>Time:</strong> {alert.Timestamp:u}</p>
<hr/>
<p>{alert.Message}</p>
{metadataHtml}";

        try
        {
            await _emailSender.SendEmailAsync(_recipientEmail, subject, body, cancellationToken);
            _logger?.LogDebug("Email alert sent: {Title}", alert.Title);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send email alert: {Title}", alert.Title);
            throw;
        }
    }
}
