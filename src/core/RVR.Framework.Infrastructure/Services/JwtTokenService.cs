using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RVR.Framework.Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace RVR.Framework.Infrastructure.Services;

/// <summary>
/// Service pour la gestion des tokens JWT.
/// Supports HS256 (symmetric) and RS256 (asymmetric) signing algorithms.
/// </summary>
public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Génère un token JWT pour un utilisateur
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"] ?? "RVR.Framework";
        var audience = jwtSettings["Audience"] ?? "RVR.Framework.Client";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var credentials = GetSigningCredentials(jwtSettings);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Ajouter le TenantId si présent
        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Génère un refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Valide un token JWT expiré (pour le refresh token flow)
    /// </summary>
    public ClaimsPrincipal? ValidateExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var signingKey = GetValidationKey(jwtSettings);

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // Accepter les tokens expirés
            }, out SecurityToken validatedToken);

            var algorithm = GetConfiguredAlgorithm(jwtSettings);
            var expectedAlg = algorithm == "RS256" ? SecurityAlgorithms.RsaSha256 : SecurityAlgorithms.HmacSha256;

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(expectedAlg, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Valide un token JWT
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"] ?? "RVR.Framework";
        var audience = jwtSettings["Audience"] ?? "RVR.Framework.Client";
        var signingKey = GetValidationKey(jwtSettings);

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the signing credentials based on the configured algorithm (HS256 or RS256).
    /// </summary>
    private SigningCredentials GetSigningCredentials(IConfigurationSection jwtSettings)
    {
        var algorithm = GetConfiguredAlgorithm(jwtSettings);
        return algorithm switch
        {
            "RS256" => GetRsaSigningCredentials(jwtSettings),
            "HS256" => GetHmacSigningCredentials(jwtSettings),
            _ => throw new InvalidOperationException($"Unsupported JWT algorithm: {algorithm}. Use HS256 or RS256.")
        };
    }

    private static SigningCredentials GetHmacSigningCredentials(IConfigurationSection jwtSettings)
    {
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is required when Algorithm is HS256.");
        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            SecurityAlgorithms.HmacSha256);
    }

    private static SigningCredentials GetRsaSigningCredentials(IConfigurationSection jwtSettings)
    {
        var keyPath = jwtSettings["RsaPrivateKeyPath"];
        if (string.IsNullOrWhiteSpace(keyPath))
            throw new InvalidOperationException("RsaPrivateKeyPath is required when Algorithm is RS256.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(keyPath));
        return new SigningCredentials(
            new RsaSecurityKey(rsa) { KeyId = GetKeyId(keyPath) },
            SecurityAlgorithms.RsaSha256);
    }

    /// <summary>
    /// Returns the validation key based on the configured algorithm.
    /// For RS256, uses the public key (or derives it from the private key).
    /// </summary>
    private static SecurityKey GetValidationKey(IConfigurationSection jwtSettings)
    {
        var algorithm = GetConfiguredAlgorithm(jwtSettings);
        return algorithm switch
        {
            "RS256" => GetRsaValidationKey(jwtSettings),
            "HS256" => GetHmacValidationKey(jwtSettings),
            _ => throw new InvalidOperationException($"Unsupported JWT algorithm: {algorithm}. Use HS256 or RS256.")
        };
    }

    private static SecurityKey GetHmacValidationKey(IConfigurationSection jwtSettings)
    {
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is required when Algorithm is HS256.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    private static SecurityKey GetRsaValidationKey(IConfigurationSection jwtSettings)
    {
        var publicKeyPath = jwtSettings["RsaPublicKeyPath"];
        var privateKeyPath = jwtSettings["RsaPrivateKeyPath"];
        var keyPath = !string.IsNullOrWhiteSpace(publicKeyPath) ? publicKeyPath : privateKeyPath;

        if (string.IsNullOrWhiteSpace(keyPath))
            throw new InvalidOperationException("RsaPrivateKeyPath or RsaPublicKeyPath is required when Algorithm is RS256.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(keyPath));
        return new RsaSecurityKey(rsa);
    }

    private static string GetConfiguredAlgorithm(IConfigurationSection jwtSettings)
    {
        return jwtSettings["Algorithm"]?.ToUpperInvariant() ?? "HS256";
    }

    /// <summary>
    /// Generates a stable key ID from the key file path for key rotation support.
    /// </summary>
    private static string GetKeyId(string keyPath)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(keyPath));
        return Convert.ToHexStringLower(hash)[..8];
    }
}
