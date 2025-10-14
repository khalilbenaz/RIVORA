using System.Security.Cryptography;
using RVR.Framework.Security.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Security.Providers;

/// <summary>
/// Implémentation par défaut de la rotation des secrets (en mémoire pour l'exemple)
/// En production, utilisez Azure Key Vault ou HashiCorp Vault.
/// </summary>
public class DefaultSecretRotationManager : ISecretRotationManager
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<DefaultSecretRotationManager> _logger;
    private const string SecretPrefix = "RVR_Secret_";

    public DefaultSecretRotationManager(IMemoryCache cache, ILogger<DefaultSecretRotationManager> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetCurrentSecretAsync(string keyName)
    {
        var cacheKey = $"{SecretPrefix}{keyName}_Current";
        if (!_cache.TryGetValue(cacheKey, out string? secret))
        {
            secret = GenerateNewSecret();
            _cache.Set(cacheKey, secret, TimeSpan.FromHours(24));
            _logger.LogInformation("Nouveau secret généré pour {KeyName}", keyName);
        }
        return await Task.FromResult(secret!);
    }

    public async Task RotateSecretAsync(string keyName)
    {
        var cacheKey = $"{SecretPrefix}{keyName}_Current";
        var oldKey = $"{SecretPrefix}{keyName}_Old";

        if (_cache.TryGetValue(cacheKey, out string? currentSecret))
        {
            _cache.Set(oldKey, currentSecret, TimeSpan.FromHours(25)); // Période de grâce
        }

        var newSecret = GenerateNewSecret();
        _cache.Set(cacheKey, newSecret, TimeSpan.FromHours(24));
        
        _logger.LogInformation("Rotation effectuée pour {KeyName}", keyName);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<string>> GetValidSecretsAsync(string keyName)
    {
        var list = new List<string>();
        
        if (_cache.TryGetValue($"{SecretPrefix}{keyName}_Current", out string? current))
            list.Add(current!);
            
        if (_cache.TryGetValue($"{SecretPrefix}{keyName}_Old", out string? old))
            list.Add(old!);

        if (!list.Any())
            list.Add(await GetCurrentSecretAsync(keyName));

        return list;
    }

    private string GenerateNewSecret()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
