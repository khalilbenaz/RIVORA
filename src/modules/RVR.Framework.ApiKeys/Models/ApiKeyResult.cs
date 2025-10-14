namespace RVR.Framework.ApiKeys.Models;

/// <summary>
/// Result model returned after creating or rotating an API key.
/// The plain-text key is only available at creation time.
/// </summary>
public sealed class ApiKeyResult
{
    /// <summary>
    /// Unique identifier for the API key record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The plain-text API key. Only populated on creation or rotation.
    /// </summary>
    public string? PlainTextKey { get; init; }

    /// <summary>
    /// A friendly name for the API key.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// When the key was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Optional expiration date.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Scopes/permissions granted to this key.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>
    /// Whether the key has been revoked.
    /// </summary>
    public bool IsRevoked { get; init; }
}
