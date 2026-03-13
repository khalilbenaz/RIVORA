using System.Net;
using System.Net.Mail;
using KBA.Framework.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KBA.Framework.Notifications.Providers;

/// <summary>
/// Implémentation SMTP du service d'email
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var host = _configuration["Email:Smtp:Host"];
            var port = int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
            var userName = _configuration["Email:Smtp:UserName"];
            var password = _configuration["Email:Smtp:Password"];
            var from = _configuration["Email:Smtp:From"] ?? userName;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = bool.Parse(_configuration["Email:Smtp:EnableSsl"] ?? "true")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from!),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email envoyé avec succès à {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de l'email à {To}", to);
            throw;
        }
    }

    public Task SendEmailWithTemplateAsync(string to, string subject, string templateName, object model)
    {
        // Ici on pourrait intégrer Scriban ou Razor pour le templating
        // Pour l'instant on fait un envoi simple avec un body placeholder
        _logger.LogWarning("SendEmailWithTemplateAsync non pleinement implémenté (templating manquant)");
        return SendEmailAsync(to, subject, $"Template {templateName} with model {model}");
    }
}
