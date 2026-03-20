using System.ComponentModel.DataAnnotations;

namespace RVR.Framework.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration for JWT authentication settings.
/// Validated at startup via <c>ValidateDataAnnotations</c> / <c>ValidateOnStart</c>.
/// </summary>
public class JwtSettings : IValidatableObject
{
    public const string SectionName = "JwtSettings";

    /// <summary>Symmetric secret key (required when Algorithm = HS256).</summary>
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "JwtSettings:Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JwtSettings:Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>Algorithm to use for JWT signing. Supported: HS256 (symmetric), RS256 (asymmetric). Default: HS256.</summary>
    public string Algorithm { get; set; } = "HS256";

    /// <summary>Path to RSA private key PEM file (required when Algorithm = RS256).</summary>
    public string? RsaPrivateKeyPath { get; set; }

    /// <summary>Path to RSA public key PEM file (optional, derived from private key if not set).</summary>
    public string? RsaPublicKeyPath { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var algorithm = Algorithm?.ToUpperInvariant() ?? "HS256";

        if (algorithm == "HS256")
        {
            if (string.IsNullOrWhiteSpace(SecretKey))
                yield return new ValidationResult("JwtSettings:SecretKey is required when Algorithm is HS256.",
                    new[] { nameof(SecretKey) });
            else if (SecretKey.Length < 32)
                yield return new ValidationResult("JwtSettings:SecretKey must be at least 32 characters.",
                    new[] { nameof(SecretKey) });
        }
        else if (algorithm == "RS256")
        {
            if (string.IsNullOrWhiteSpace(RsaPrivateKeyPath))
                yield return new ValidationResult("JwtSettings:RsaPrivateKeyPath is required when Algorithm is RS256.",
                    new[] { nameof(RsaPrivateKeyPath) });
        }
        else
        {
            yield return new ValidationResult($"Unsupported JWT algorithm: {Algorithm}. Use HS256 or RS256.",
                new[] { nameof(Algorithm) });
        }
    }
}
