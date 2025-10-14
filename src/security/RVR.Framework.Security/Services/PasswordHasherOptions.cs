namespace RVR.Framework.Security.Services;

/// <summary>
/// Configuration options for the <see cref="PasswordHasher"/> service.
/// </summary>
public class PasswordHasherOptions
{
    /// <summary>
    /// Gets or sets the BCrypt work factor (cost parameter).
    /// Higher values are more secure but slower. OWASP recommends a minimum of 12.
    /// Valid range: 4 to 31. Default: 12.
    /// </summary>
    public int WorkFactor { get; set; } = 12;
}
