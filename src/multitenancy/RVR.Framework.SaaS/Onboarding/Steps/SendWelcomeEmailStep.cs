using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;

namespace RVR.Framework.SaaS.Onboarding.Steps;

/// <summary>
/// Abstraction for sending welcome emails during tenant onboarding.
/// Implement and register this interface in DI to enable email delivery;
/// when unregistered the step falls back to logging.
/// </summary>
public interface IWelcomeEmailSender
{
    /// <summary>
    /// Sends a welcome email to the specified recipient.
    /// </summary>
    Task SendWelcomeEmailAsync(string to, string subject, string body, bool isHtml);
}

/// <summary>
/// Sends a welcome email to the tenant's admin user. Attempts to use
/// <c>IEmailService</c> (from RVR.Framework.Notifications) when registered;
/// falls back to logging the welcome message when the service is unavailable.
/// This step is idempotent — compensation is a no-op since sent emails cannot be recalled.
/// </summary>
public sealed class SendWelcomeEmailStep : ITenantOnboardingStep
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SendWelcomeEmailStep> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SendWelcomeEmailStep"/>.
    /// </summary>
    /// <param name="serviceProvider">Service provider used to resolve optional email services at runtime.</param>
    /// <param name="logger">Logger instance.</param>
    public SendWelcomeEmailStep(
        IServiceProvider serviceProvider,
        ILogger<SendWelcomeEmailStep> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string Name => "SendWelcomeEmail";

    /// <inheritdoc />
    public int Order => 400;

    /// <inheritdoc />
    public async Task<OnboardingStepResult> ExecuteAsync(
        TenantOnboardingContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(context.AdminEmail))
        {
            return new OnboardingStepResult(Name, false, "Admin email is required to send the welcome email.");
        }

        var subject = $"Welcome to {context.TenantName}!";
        var body = BuildWelcomeBody(context);

        // Resolve IWelcomeEmailSender from DI (Native AOT-safe, no reflection).
        var emailSender = _serviceProvider.GetService<IWelcomeEmailSender>();

        if (emailSender is not null)
        {
            try
            {
                await emailSender.SendWelcomeEmailAsync(context.AdminEmail, subject, body, isHtml: true);

                _logger.LogInformation(
                    "Welcome email sent to {AdminEmail} for tenant {TenantId} via IWelcomeEmailSender",
                    LogSanitizer.Sanitize(context.AdminEmail), context.TenantId);

                return new OnboardingStepResult(Name, true, "Welcome email sent via IWelcomeEmailSender");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "IWelcomeEmailSender failed for tenant {TenantId}. Falling back to log-only",
                    context.TenantId);
            }
        }

        // Fallback: log the welcome message.
        _logger.LogInformation(
            "Welcome email (log-only) for tenant {TenantId}: To={AdminEmail}, Subject={Subject}",
            context.TenantId, LogSanitizer.Sanitize(context.AdminEmail), LogSanitizer.Sanitize(subject));

        return new OnboardingStepResult(Name, true, "Welcome email logged (no email service registered)");
    }

    /// <inheritdoc />
    public Task CompensateAsync(TenantOnboardingContext context, CancellationToken ct = default)
    {
        // Emails cannot be unsent. Compensation is a no-op.
        _logger.LogInformation(
            "Compensating SendWelcomeEmail: no action required (emails cannot be recalled) for tenant {TenantId}",
            context.TenantId);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Builds the HTML welcome email body.
    /// </summary>
    private static string BuildWelcomeBody(TenantOnboardingContext context)
    {
        var passwordSection = string.IsNullOrWhiteSpace(context.AdminPassword)
            ? string.Empty
            : $"<p>Your temporary password is: <strong>{context.AdminPassword}</strong></p>" +
              "<p>Please change it after your first login.</p>";

        return $"""
            <html>
            <body>
                <h1>Welcome to {context.TenantName}!</h1>
                <p>Your tenant has been successfully provisioned on the <strong>{context.Plan}</strong> plan.</p>
                <p>Admin email: <strong>{context.AdminEmail}</strong></p>
                {passwordSection}
                <p>You can now log in and start configuring your workspace.</p>
                <br/>
                <p>— The Rivora Platform Team</p>
            </body>
            </html>
            """;
    }
}
