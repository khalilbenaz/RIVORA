# Identity.Pro

The Identity.Pro module extends the base identity system with advanced enterprise features: session management, admin impersonation, and OAuth2/OIDC integration.

## User Entity

The framework's `User` entity supports the full identity lifecycle:

```csharp
public class User : FullAuditedEntity<Guid>
{
    public Guid? TenantId { get; }
    public string UserName { get; }
    public string Email { get; }
    public bool EmailConfirmed { get; }
    public bool TwoFactorEnabled { get; }
    public bool LockoutEnabled { get; }
    public int FailedLoginAttempts { get; }
    public DateTime? LockoutEndUtc { get; }
    public bool IsActive { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public string FullName { get; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; }
    public ICollection<UserClaim> UserClaims { get; }
    public ICollection<UserLogin> UserLogins { get; }    // External logins
    public ICollection<UserToken> UserTokens { get; }
}
```

## Session Management

Track and manage active user sessions:

```csharp
public interface ISessionManager
{
    Task<UserSession> CreateSessionAsync(Guid userId, string ipAddress, string userAgent, CancellationToken ct = default);
    Task<IReadOnlyList<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default);
}
```

### Listing active sessions

```csharp
[HttpGet("sessions")]
[Authorize]
public async Task<IActionResult> GetMySessions(CancellationToken ct)
{
    var userId = User.GetUserId();
    var sessions = await _sessionManager.GetActiveSessionsAsync(userId, ct);
    return Ok(sessions);
}

[HttpDelete("sessions/{sessionId}")]
[Authorize]
public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken ct)
{
    await _sessionManager.RevokeSessionAsync(sessionId, ct);
    return NoContent();
}

// Revoke all other sessions (keep current one)
[HttpPost("sessions/revoke-others")]
[Authorize]
public async Task<IActionResult> RevokeOtherSessions(CancellationToken ct)
{
    var userId = User.GetUserId();
    await _sessionManager.RevokeAllSessionsAsync(userId, ct);
    return Ok(new { message = "All other sessions have been revoked." });
}
```

## Admin Impersonation

Allows administrators to impersonate other users for debugging and support:

```csharp
public interface IImpersonationService
{
    Task<AuthResponseDto> ImpersonateAsync(Guid adminUserId, Guid targetUserId, CancellationToken ct = default);
    Task StopImpersonationAsync(CancellationToken ct = default);
    bool IsImpersonating { get; }
    Guid? OriginalUserId { get; }
}
```

### Impersonation flow

```csharp
[HttpPost("impersonate/{userId}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Impersonate(Guid userId, CancellationToken ct)
{
    var adminId = User.GetUserId();
    var response = await _impersonationService.ImpersonateAsync(adminId, userId, ct);

    // Returns a new JWT with the target user's claims
    // plus an "OriginalUserId" claim for audit purposes
    return Ok(response);
}

[HttpPost("stop-impersonation")]
[Authorize]
public async Task<IActionResult> StopImpersonation(CancellationToken ct)
{
    await _impersonationService.StopImpersonationAsync(ct);
    return Ok();
}
```

Impersonation is fully audited -- all actions taken while impersonating are logged with both the admin and target user IDs.

## OAuth2/OIDC Integration

Configure external identity providers:

### Azure AD

```csharp
builder.Services.AddAuthentication()
    .AddOpenIdConnect("AzureAD", options =>
    {
        options.Authority = "https://login.microsoftonline.com/{tenant-id}/v2.0";
        options.ClientId = builder.Configuration["AzureAd:ClientId"];
        options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });
```

### Keycloak

```csharp
builder.Services.AddAuthentication()
    .AddOpenIdConnect("Keycloak", options =>
    {
        options.Authority = "https://keycloak.example.com/realms/rivora";
        options.ClientId = "rivora-api";
        options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
        options.ResponseType = "code";
        options.RequireHttpsMetadata = true;
    });
```

### Claims Transformation

Map external claims to RIVORA Framework claims:

```csharp
builder.Services.AddRvrClaimsTransformation(options =>
{
    options.MapClaim("preferred_username", ClaimTypes.Name);
    options.MapClaim("email", ClaimTypes.Email);
    options.MapClaim("groups", "Role");
    options.MapClaim("tenant_id", "TenantId");
});
```

## Account Lockout (Anti Brute-Force)

The User entity includes built-in brute-force protection:

```csharp
// Automatic lockout after failed attempts
user.IncrementFailedLogins();

if (user.FailedLoginAttempts >= maxAttempts)
{
    user.LockUntil(DateTime.UtcNow.AddMinutes(lockoutDurationMinutes));
}

// Check lockout
if (user.IsLockedOut())
{
    return Unauthorized("Account is locked. Try again later.");
}

// Reset on successful login
user.ResetFailedLogins();
```

## Registration

```csharp
builder.Services.AddRvrIdentityPro(options =>
{
    options.EnableSessionManagement = true;
    options.MaxConcurrentSessions = 5;
    options.EnableImpersonation = true;
    options.ImpersonationAuditLevel = AuditLevel.Full;
    options.LockoutMaxAttempts = 5;
    options.LockoutDurationMinutes = 15;
});
```
