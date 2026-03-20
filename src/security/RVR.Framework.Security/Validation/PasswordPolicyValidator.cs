namespace RVR.Framework.Security.Validation;

using Microsoft.Extensions.Options;

/// <summary>
/// Configurable password policy validator following OWASP guidelines.
/// </summary>
public class PasswordPolicyValidator : IPasswordPolicyValidator
{
    private readonly PasswordPolicyOptions _options;

    public PasswordPolicyValidator(IOptions<PasswordPolicyOptions> options)
    {
        _options = options?.Value ?? new PasswordPolicyOptions();
    }

    public PasswordValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return new PasswordValidationResult(false, errors);
        }

        if (password.Length < _options.MinimumLength)
            errors.Add($"Password must be at least {_options.MinimumLength} characters long.");

        if (password.Length > _options.MaximumLength)
            errors.Add($"Password must not exceed {_options.MaximumLength} characters.");

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter.");

        if (_options.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter.");

        if (_options.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit.");

        if (_options.RequireSpecialCharacter && !password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("Password must contain at least one special character.");

        // Check for common weak passwords
        if (_options.RejectCommonPasswords && IsCommonPassword(password))
            errors.Add("This password is too common. Please choose a stronger password.");

        return new PasswordValidationResult(errors.Count == 0, errors);
    }

    private static bool IsCommonPassword(string password)
    {
        var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "123456", "12345678", "qwerty", "abc123",
            "monkey", "1234567", "letmein", "trustno1", "dragon",
            "baseball", "iloveyou", "master", "sunshine", "ashley",
            "football", "shadow", "123123", "654321", "superman",
            "qazwsx", "michael", "password1", "password123", "admin",
            "welcome", "login", "passw0rd", "p@ssword", "changeme"
        };
        return commonPasswords.Contains(password);
    }
}

public class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";
    public int MinimumLength { get; set; } = 12;
    public int MaximumLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
    public bool RejectCommonPasswords { get; set; } = true;
}

public interface IPasswordPolicyValidator
{
    PasswordValidationResult Validate(string password);
}

public record PasswordValidationResult(bool IsValid, IReadOnlyList<string> Errors);
