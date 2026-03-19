using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.Email.Models;

namespace RVR.Framework.Email.Services;

/// <summary>
/// SMTP-based implementation of <see cref="IEmailSender"/> using System.Net.Mail.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender, IDisposable
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly SmtpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailSender"/> class.
    /// </summary>
    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl
        };

        if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            _client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromAddress),
            Subject = message.Subject,
            IsBodyHtml = message.HtmlBody is not null
        };

        foreach (var to in message.To)
        {
            mailMessage.To.Add(to);
        }

        if (message.HtmlBody is not null)
        {
            mailMessage.Body = message.HtmlBody;

            if (message.TextBody is not null)
            {
                using var plainView = AlternateView.CreateAlternateViewFromString(
                    message.TextBody, null, "text/plain");
                mailMessage.AlternateViews.Add(plainView);
            }
        }
        else if (message.TextBody is not null)
        {
            mailMessage.Body = message.TextBody;
            mailMessage.IsBodyHtml = false;
        }

        var streams = new List<MemoryStream>();
        try
        {
            foreach (var attachment in message.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                streams.Add(stream);
                mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }

            _logger.LogInformation("Sending email to {Recipients} with subject '{Subject}'",
                string.Join(", ", message.To), message.Subject);

            await _client.SendMailAsync(mailMessage, ct);
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }
}
