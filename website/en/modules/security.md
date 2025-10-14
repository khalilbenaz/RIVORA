# Security Module

**Packages**: `RVR.Framework.Security`, `RVR.Framework.Identity.Pro`

Complete security management: JWT authentication, BCrypt hashing, 2FA/TOTP, AES-256 encryption, account lockout, rate limiting and audit trail.

```csharp
builder.Services.AddRvrSecurity(builder.Configuration);
builder.Services.AddRvrAuthentication(builder.Configuration);
builder.Services.AddRvrAuthorization();
```

See the [Security guide](/en/guide/security) for configuration details.
