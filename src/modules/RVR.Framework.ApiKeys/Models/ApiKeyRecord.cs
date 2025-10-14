namespace RVR.Framework.ApiKeys.Models;

/// <summary>
/// Internal storage record for an API key. Stores the SHA-256 hash, never the plain key.
/// </summary>
internal sealed class ApiKeyRecord
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string KeyHash { get; set; }
    public required string KeyPrefix { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public IReadOnlyList<string> Scopes { get; set; } = [];
    public bool IsRevoked { get; set; }
}
