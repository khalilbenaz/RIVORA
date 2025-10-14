using RVR.Framework.Email.Models;

namespace RVR.Framework.Email.Services;

/// <summary>
/// Abstraction for sending emails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
