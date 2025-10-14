namespace RVR.Framework.ApiKeys.Models;

/// <summary>
/// Request model for creating a new API key.
/// </summary>
public sealed class ApiKeyCreateRequest
{
    /// <summary>
    /// A friendly name for the API key.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional expiration date for the key.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Optional set of scopes/permissions granted to this key.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];
}
