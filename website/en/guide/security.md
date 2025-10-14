# Security

## Overview

RIVORA Framework integrates security by design, covering authentication, authorization, encryption and auditing.

| Feature | Implementation |
|---------|---------------|
| JWT + Refresh Tokens | Automatic rotation and revocation |
| BCrypt Password Hashing | Work factor 12, OWASP compliant |
| Account Lockout | 5 attempts, 15 min lockout |
| 2FA/TOTP | QR Code + backup codes |
| AES-256 Encryption | At-rest encryption via attribute |
| Rate Limiting | IP-based and user-based |
| Audit Trail | Automatic EF Core interceptor |
| OWASP Headers | CSP, HSTS, X-Frame-Options |

## JWT Authentication

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

### Authentication Flow

1. `POST /api/v1/auth/login` with email/password
2. Server verifies BCrypt, returns `accessToken` + `refreshToken`
3. Client sends `Authorization: Bearer <token>` on each request
4. On expiration, `POST /api/v1/auth/refresh` with the `refreshToken`
5. Old refresh token is revoked (rotation)

## Account Lockout

```json
{
  "SecuritySettings": {
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 15,
    "EnableAccountLockout": true
  }
}
```

After 5 failed attempts, the account is locked for 15 minutes. An `AccountLockedOutEvent` is emitted for notification.

## Rate Limiting

```csharp
builder.Services.AddRvrRateLimiting(options =>
{
    options.GlobalLimit = 100;
    options.PerUserLimit = 30;
    options.PerIpLimit = 50;
});
```

## AES-256 Encryption

```csharp
public class Patient
{
    public string Name { get; set; }

    [EncryptedAtRest]
    public string SocialSecurityNumber { get; set; }
}
```
