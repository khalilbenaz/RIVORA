namespace RVR.Framework.Notifications.Services;

/// <summary>
/// Service pour l'envoi d'emails
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailWithTemplateAsync(string to, string subject, string templateName, object model);
}
