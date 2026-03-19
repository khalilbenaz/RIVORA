using System.Text.RegularExpressions;

namespace RVR.Framework.Email.Templates;

/// <summary>
/// Renders built-in HTML email templates with variable substitution.
/// Templates use inline CSS for maximum email-client compatibility (Gmail, Outlook, etc.).
/// </summary>
public partial class InlineEmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly Dictionary<string, string> Templates = new()
    {
        ["welcome"] = BuildTemplate(
            "Welcome to {{appName}}!",
            """
            <h2 style="margin:0 0 16px;font-size:22px;color:#1e293b;">Welcome to {{appName}}!</h2>
            <p style="margin:0 0 12px;font-size:15px;color:#475569;line-height:1.6;">
              Your account has been created successfully. You're all set to start exploring everything {{appName}} has to offer.
            </p>
            <p style="margin:0 0 24px;font-size:15px;color:#475569;line-height:1.6;">
              Click the button below to get started.
            </p>
            """,
            "Get Started",
            "{{dashboardUrl}}"
        ),

        ["password-reset"] = BuildTemplate(
            "Reset Your Password",
            """
            <h2 style="margin:0 0 16px;font-size:22px;color:#1e293b;">Reset Your Password</h2>
            <p style="margin:0 0 12px;font-size:15px;color:#475569;line-height:1.6;">
              We received a request to reset your password. Use the button below to set a new one.
            </p>
            <p style="margin:0 0 8px;font-size:13px;color:#64748b;">
              Your reset token: <code style="background:#f1f5f9;padding:2px 6px;border-radius:4px;font-size:13px;">{{resetToken}}</code>
            </p>
            <p style="margin:0 0 24px;font-size:13px;color:#94a3b8;">
              If you did not request a password reset, you can safely ignore this email.
            </p>
            """,
            "Reset Password",
            "{{resetUrl}}"
        ),

        ["email-verification"] = BuildTemplate(
            "Verify Your Email",
            """
            <h2 style="margin:0 0 16px;font-size:22px;color:#1e293b;">Verify Your Email Address</h2>
            <p style="margin:0 0 12px;font-size:15px;color:#475569;line-height:1.6;">
              Thanks for signing up! Please verify your email address by clicking the button below.
            </p>
            <p style="margin:0 0 24px;font-size:13px;color:#94a3b8;">
              If you did not create an account, no further action is required.
            </p>
            """,
            "Verify Email",
            "{{verifyUrl}}"
        ),

        ["invoice"] = BuildTemplate(
            "Invoice #{{invoiceNumber}}",
            """
            <h2 style="margin:0 0 16px;font-size:22px;color:#1e293b;">Invoice #{{invoiceNumber}}</h2>
            <table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
              <tr>
                <td style="padding:8px 0;font-size:14px;color:#64748b;border-bottom:1px solid #e2e8f0;">Amount</td>
                <td style="padding:8px 0;font-size:14px;color:#1e293b;text-align:right;border-bottom:1px solid #e2e8f0;font-weight:600;">{{amount}}</td>
              </tr>
              <tr>
                <td style="padding:8px 0;font-size:14px;color:#64748b;border-bottom:1px solid #e2e8f0;">Due Date</td>
                <td style="padding:8px 0;font-size:14px;color:#1e293b;text-align:right;border-bottom:1px solid #e2e8f0;">{{dueDate}}</td>
              </tr>
              <tr>
                <td style="padding:8px 0;font-size:14px;color:#64748b;">Invoice Number</td>
                <td style="padding:8px 0;font-size:14px;color:#1e293b;text-align:right;">{{invoiceNumber}}</td>
              </tr>
            </table>
            """,
            "View Invoice",
            "{{invoiceUrl}}"
        ),
    };

    /// <inheritdoc />
    public string Render(string templateName, Dictionary<string, string> variables)
    {
        if (!Templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Email template '{templateName}' not found. Available templates: {string.Join(", ", Templates.Keys)}");
        }

        return VariablePlaceholderRegex().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePlaceholderRegex();

    private static string BuildTemplate(string title, string bodyContent, string ctaText, string ctaUrl)
    {
        return """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>TEMPLATE_TITLE</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f1f5f9;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f1f5f9;">
            <tr>
              <td align="center" style="padding:32px 16px;">
                <table role="presentation" width="600" cellspacing="0" cellpadding="0" border="0" style="max-width:600px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,0.1);">
                  <!-- Header -->
                  <tr>
                    <td style="background-color:#3b82f6;padding:28px 32px;text-align:center;">
                      <h1 style="margin:0;font-size:24px;font-weight:700;color:#ffffff;letter-spacing:-0.5px;">RIVORA</h1>
                    </td>
                  </tr>
                  <!-- Content -->
                  <tr>
                    <td style="padding:32px;">
                      TEMPLATE_BODY
                      <!-- CTA Button -->
                      <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:0 auto;">
                        <tr>
                          <td style="border-radius:8px;background-color:#3b82f6;">
                            <a href="TEMPLATE_CTA_URL" target="_blank" style="display:inline-block;padding:14px 32px;font-size:15px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:8px;">TEMPLATE_CTA_TEXT</a>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                  <!-- Footer -->
                  <tr>
                    <td style="padding:24px 32px;background-color:#f8fafc;border-top:1px solid #e2e8f0;text-align:center;">
                      <p style="margin:0 0 8px;font-size:12px;color:#94a3b8;">
                        You received this email because you have an account with RIVORA.
                      </p>
                      <p style="margin:0;font-size:12px;color:#94a3b8;">
                        If you wish to stop receiving these emails, you can <a href="{{unsubscribeUrl}}" style="color:#3b82f6;text-decoration:underline;">unsubscribe</a>.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """
        .Replace("TEMPLATE_TITLE", title)
        .Replace("TEMPLATE_BODY", bodyContent)
        .Replace("TEMPLATE_CTA_URL", ctaUrl)
        .Replace("TEMPLATE_CTA_TEXT", ctaText);
    }
}
