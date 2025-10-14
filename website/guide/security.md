# Securite

## Vue d'ensemble

RIVORA Framework integre une securite "By Design" couvrant authentification, autorisation, chiffrement et audit.

| Feature | Implementation |
|---------|---------------|
| JWT + Refresh Tokens | Rotation et revocation automatique |
| BCrypt Password Hashing | Work factor 12, OWASP compliant |
| Account Lockout | 5 tentatives, 15 min lockout |
| 2FA/TOTP | QR Code + backup codes |
| AES-256 Encryption | Chiffrement at-rest via attribut |
| Rate Limiting | IP-based et user-based |
| Audit Trail | Intercepteur EF Core automatique |
| OWASP Headers | CSP, HSTS, X-Frame-Options |

## Authentification JWT

### Configuration

```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-here-minimum-32-chars",
    "Issuer": "RVR.Framework",
    "Audience": "RVR.Framework.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Flux d'authentification

1. `POST /api/v1/auth/login` avec email/password
2. Le serveur verifie BCrypt, retourne `accessToken` + `refreshToken`
3. Le client envoie `Authorization: Bearer <token>` a chaque requete
4. A expiration, `POST /api/v1/auth/refresh` avec le `refreshToken`
5. L'ancien refresh token est revoque (rotation)

### Enregistrement

```csharp
builder.Services.AddRvrAuthentication(builder.Configuration);
builder.Services.AddRvrAuthorization();
```

## Account Lockout

Protection anti brute-force configurable :

```json
{
  "SecuritySettings": {
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 15,
    "EnableAccountLockout": true
  }
}
```

Apres 5 tentatives echouees, le compte est verrouille 15 minutes. Un event `AccountLockedOutEvent` est emis pour notification.

## Rate Limiting

```csharp
builder.Services.AddRvrRateLimiting(options =>
{
    options.GlobalLimit = 100;           // requetes/minute global
    options.PerUserLimit = 30;           // requetes/minute par user
    options.PerIpLimit = 50;             // requetes/minute par IP
    options.EnableTrustedProxySupport = true;
});
```

Endpoints specifiques :

```csharp
app.MapPost("/api/v1/auth/login", LoginHandler)
   .RequireRateLimiting("auth", limit: 5, window: TimeSpan.FromMinutes(1));
```

## Chiffrement AES-256

Attribut pour chiffrer les proprietes en base :

```csharp
public class Patient
{
    public string Name { get; set; }

    [EncryptedAtRest]
    public string SocialSecurityNumber { get; set; }

    [EncryptedAtRest]
    public string MedicalNotes { get; set; }
}
```

Configuration :

```json
{
  "EncryptionSettings": {
    "Key": "your-256-bit-encryption-key",
    "Algorithm": "AES-256-CBC"
  }
}
```

## Audit Trail

L'intercepteur EF Core capture automatiquement les modifications :

```csharp
builder.Services.AddRvrAuditLogging(options =>
{
    options.EnableAutoCapture = true;
    options.CaptureEntityChanges = true;
    options.IncludeProperties = true;
});
```

Chaque operation genere un `AuditEntry` avec : qui, quoi, quand, anciennes/nouvelles valeurs.

## OWASP Headers

Appliques automatiquement en production :

```csharp
app.UseRvrSecurityHeaders(); // CSP, HSTS, X-Frame-Options, X-Content-Type-Options
```
