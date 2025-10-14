namespace RVR.Framework.Email.Models;

/// <summary>
/// Configuration options for the SMTP email sender.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// The SMTP server host.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// The SMTP server port.
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// The SMTP username for authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The SMTP password for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The sender email address.
    /// </summary>
    public required string FromAddress { get; set; }

    /// <summary>
    /// Whether to use SSL/TLS.
    /// </summary>
    public bool UseSsl { get; set; } = true;
}
