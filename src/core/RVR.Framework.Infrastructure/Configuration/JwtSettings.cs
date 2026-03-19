using System.ComponentModel.DataAnnotations;

namespace RVR.Framework.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration for JWT authentication settings.
/// Validated at startup via <c>ValidateDataAnnotations</c> / <c>ValidateOnStart</c>.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required(ErrorMessage = "JwtSettings:SecretKey is required.")]
    [MinLength(32, ErrorMessage = "JwtSettings:SecretKey must be at least 32 characters.")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "JwtSettings:Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JwtSettings:Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationDays { get; set; } = 7;
}
