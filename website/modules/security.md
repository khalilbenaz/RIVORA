# Module Security

**Packages** : `RVR.Framework.Security`, `RVR.Framework.Identity.Pro`

## Description

Gestion complete de la securite : authentification JWT, hachage BCrypt, 2FA/TOTP, chiffrement AES-256, account lockout, rate limiting et audit trail.

## Enregistrement

```csharp
builder.Services.AddRvrSecurity(builder.Configuration);
builder.Services.AddRvrAuthentication(builder.Configuration);
builder.Services.AddRvrAuthorization();
builder.Services.AddRvrRateLimiting(builder.Configuration);
builder.Services.AddRvrAuditLogging();
```

## Interfaces principales

```csharp
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
```

## Configuration

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-chars",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "SecuritySettings": {
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 15,
    "PasswordMinLength": 8,
    "RequireUppercase": true,
    "RequireDigit": true,
    "RequireSpecialChar": true
  }
}
```

Voir le [guide Securite](/guide/security) pour les details d'implementation.
