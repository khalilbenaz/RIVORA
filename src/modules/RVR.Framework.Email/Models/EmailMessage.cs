namespace RVR.Framework.Email.Models;

/// <summary>
/// Represents an email message to be sent.
/// </summary>
public sealed class EmailMessage
{
    /// <summary>
    /// Recipient email addresses.
    /// </summary>
    public required string[] To { get; init; }

    /// <summary>
    /// The email subject line.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// The HTML body of the email.
    /// </summary>
    public string? HtmlBody { get; init; }

    /// <summary>
    /// The plain-text body of the email.
    /// </summary>
    public string? TextBody { get; init; }

    /// <summary>
    /// Optional file attachments.
    /// </summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; init; } = [];
}

/// <summary>
/// Represents an email attachment.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// The file name of the attachment.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The content of the attachment.
    /// </summary>
    public required byte[] Content { get; init; }

    /// <summary>
    /// The MIME content type.
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";
}
