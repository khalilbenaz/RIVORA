using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using RVR.Framework.ApiKeys.Models;

namespace RVR.Framework.ApiKeys.Services;

/// <summary>
/// In-memory implementation of <see cref="IApiKeyService"/>.
/// Stores SHA-256 hashes of keys; never stores the plain-text key.
/// </summary>
public sealed class ApiKeyService : IApiKeyService
{
    private readonly ConcurrentDictionary<Guid, ApiKeyRecord> _keys = new();
    private readonly ILogger<ApiKeyService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyService"/> class.
    /// </summary>
    public ApiKeyService(ILogger<ApiKeyService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ApiKeyResult> GenerateKeyAsync(ApiKeyCreateRequest request, CancellationToken ct = default)
    {
        var plainTextKey = GeneratePlainTextKey();
        var hash = ComputeSha256Hash(plainTextKey);
        var prefix = plainTextKey[..8];

        var record = new ApiKeyRecord
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            KeyHash = hash,
            KeyPrefix = prefix,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt,
            Scopes = request.Scopes
        };

        _keys[record.Id] = record;
        _logger.LogInformation("API key generated: {KeyId} ({Prefix}...)", record.Id, prefix);

        return Task.FromResult(ToResult(record, plainTextKey));
    }

    /// <inheritdoc />
    public Task<ApiKeyResult?> ValidateKeyAsync(string plainTextKey, CancellationToken ct = default)
    {
        var hash = ComputeSha256Hash(plainTextKey);

        foreach (var record in _keys.Values)
        {
            if (record.KeyHash != hash) continue;
            if (record.IsRevoked) return Task.FromResult<ApiKeyResult?>(null);
            if (record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTimeOffset.UtcNow)
                return Task.FromResult<ApiKeyResult?>(null);

            return Task.FromResult<ApiKeyResult?>(ToResult(record));
        }

        return Task.FromResult<ApiKeyResult?>(null);
    }

    /// <inheritdoc />
    public Task RevokeKeyAsync(Guid keyId, CancellationToken ct = default)
    {
        if (_keys.TryGetValue(keyId, out var record))
        {
            record.IsRevoked = true;
            _logger.LogInformation("API key revoked: {KeyId}", keyId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<ApiKeyResult> RotateKeyAsync(Guid keyId, CancellationToken ct = default)
    {
        if (!_keys.TryGetValue(keyId, out var oldRecord))
            throw new InvalidOperationException($"API key not found: {keyId}");

        await RevokeKeyAsync(keyId, ct);

        var request = new ApiKeyCreateRequest
        {
            Name = oldRecord.Name,
            ExpiresAt = oldRecord.ExpiresAt,
            Scopes = oldRecord.Scopes
        };

        return await GenerateKeyAsync(request, ct);
    }

    private static string GeneratePlainTextKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return $"rvr_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private static ApiKeyResult ToResult(ApiKeyRecord record, string? plainTextKey = null) => new()
    {
        Id = record.Id,
        PlainTextKey = plainTextKey,
        Name = record.Name,
        CreatedAt = record.CreatedAt,
        ExpiresAt = record.ExpiresAt,
        Scopes = record.Scopes,
        IsRevoked = record.IsRevoked
    };
}
