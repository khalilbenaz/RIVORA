using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;

namespace RVR.Framework.SaaS.Onboarding.Steps;

/// <summary>
/// Creates the initial admin user for the newly provisioned tenant.
/// When no password is provided in the context, a cryptographically secure random password
/// is generated and stored in the context properties for downstream steps (e.g., welcome email).
/// </summary>
public sealed class CreateAdminUserStep : ITenantOnboardingStep
{
    private readonly ILogger<CreateAdminUserStep> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateAdminUserStep"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public CreateAdminUserStep(ILogger<CreateAdminUserStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string Name => "CreateAdminUser";

    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public Task<OnboardingStepResult> ExecuteAsync(
        TenantOnboardingContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(context.AdminEmail))
        {
            return Task.FromResult(
                new OnboardingStepResult(Name, false, "Admin email is required to create the admin user."));
        }

        // Generate a password if one was not provided.
        var password = context.AdminPassword;
        if (string.IsNullOrWhiteSpace(password))
        {
            password = GenerateSecurePassword();
            context.AdminPassword = password;
        }

        // In production, this would call an identity service (e.g., ASP.NET Core Identity)
        // to create the user, assign the "Admin" role, and link them to the tenant.
        context.Properties["AdminUserId"] = Guid.NewGuid();
        context.Properties["AdminPasswordGenerated"] = string.IsNullOrWhiteSpace(context.AdminPassword) is false;

        _logger.LogInformation(
            "Admin user created for tenant {TenantId} with email {AdminEmail}",
            context.TenantId, LogSanitizer.Sanitize(context.AdminEmail));

        return Task.FromResult(
            new OnboardingStepResult(
                Name,
                true,
                "Admin user created successfully",
                new Dictionary<string, object>
                {
                    ["AdminUserId"] = context.Properties["AdminUserId"],
                    ["AdminEmail"] = context.AdminEmail
                }));
    }

    /// <inheritdoc />
    public Task CompensateAsync(TenantOnboardingContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Properties.TryGetValue("AdminUserId", out var userIdObj))
        {
            // In production, delete or deactivate the admin user via the identity service.
            _logger.LogInformation(
                "Compensating CreateAdminUser: removing admin user {AdminUserId} for tenant {TenantId}",
                userIdObj, context.TenantId);

            context.Properties.Remove("AdminUserId");
        }
        else
        {
            _logger.LogWarning(
                "Compensating CreateAdminUser: no AdminUserId found in context for tenant {TenantId}",
                context.TenantId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Generates a cryptographically secure password that satisfies typical complexity requirements.
    /// </summary>
    private static string GenerateSecurePassword(int length = 16)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string all = upper + lower + digits + special;

        Span<char> password = stackalloc char[length];

        // Guarantee at least one character from each category.
        password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (var i = 4; i < length; i++)
        {
            password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Shuffle to avoid predictable positions of required character types.
        for (var i = length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
