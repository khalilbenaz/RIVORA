using RVR.Framework.ApiKeys.Models;

namespace RVR.Framework.ApiKeys.Services;

/// <summary>
/// Service for managing API keys.
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Generates a new API key.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result containing the plain-text key (only available at creation).</returns>
    Task<ApiKeyResult> GenerateKeyAsync(ApiKeyCreateRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates an API key and returns the associated record if valid.
    /// </summary>
    /// <param name="plainTextKey">The plain-text API key to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The key result if valid; null otherwise.</returns>
    Task<ApiKeyResult?> ValidateKeyAsync(string plainTextKey, CancellationToken ct = default);

    /// <summary>
    /// Revokes an API key by its identifier.
    /// </summary>
    /// <param name="keyId">The key identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RevokeKeyAsync(Guid keyId, CancellationToken ct = default);

    /// <summary>
    /// Rotates an API key, revoking the old one and generating a new one.
    /// </summary>
    /// <param name="keyId">The identifier of the key to rotate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result containing the new plain-text key.</returns>
    Task<ApiKeyResult> RotateKeyAsync(Guid keyId, CancellationToken ct = default);
}
