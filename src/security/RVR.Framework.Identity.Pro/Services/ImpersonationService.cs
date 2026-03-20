using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RVR.Framework.Identity.Pro.Interfaces;

namespace RVR.Framework.Identity.Pro.Services;

/// <summary>
/// Allows an admin user to impersonate another user by generating a special JWT token
/// that preserves the original admin identity via the "impersonator_id" claim.
/// </summary>
public class ImpersonationService : IImpersonationService
{
    /// <summary>
    /// The claim type used to store the original admin user ID during impersonation.
    /// </summary>
    public const string ImpersonatorClaimType = "impersonator_id";

    private readonly ILogger<ImpersonationService> _logger;
    private readonly string _signingKey;
    private readonly string _issuer;
    private readonly string _audience;

    public ImpersonationService(
        ILogger<ImpersonationService> logger,
        string signingKey,
        string issuer = "RVR.Framework.Identity.Pro",
        string audience = "RVR.Framework")
    {
        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException("Impersonation signing key must be configured. Set it via 'JwtSettings__SecretKey' environment variable.");

        _logger = logger;
        _signingKey = signingKey;
        _issuer = issuer;
        _audience = audience;
    }

    public async Task<string> ImpersonateUserAsync(
        string adminUserId,
        string targetTenantId,
        string targetUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetUserId);

        _logger.LogInformation(
            "Admin {AdminUserId} is impersonating user {TargetUserId} in tenant {TenantId}",
            adminUserId, targetUserId, targetTenantId);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, targetUserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", targetTenantId ?? string.Empty),
            new(ImpersonatorClaimType, adminUserId),
            new("is_impersonation", "true")
        };

        var token = GenerateJwtToken(claims, TimeSpan.FromHours(1));

        await Task.CompletedTask; // Placeholder for async audit logging

        return token;
    }

    public async Task<string> StopImpersonationAsync(string currentToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentToken);

        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(currentToken))
            throw new InvalidOperationException("Invalid impersonation token.");

        var jwt = tokenHandler.ReadJwtToken(currentToken);
        var impersonatorId = jwt.Claims.FirstOrDefault(c => c.Type == ImpersonatorClaimType)?.Value;

        if (string.IsNullOrEmpty(impersonatorId))
            throw new InvalidOperationException("Token does not contain an impersonator claim. Not an impersonation session.");

        _logger.LogInformation("Stopping impersonation, restoring admin {AdminUserId}", impersonatorId);

        // Generate a fresh token for the original admin
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, impersonatorId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = GenerateJwtToken(claims, TimeSpan.FromHours(8));

        await Task.CompletedTask; // Placeholder for async audit logging

        return token;
    }

    private string GenerateJwtToken(IEnumerable<Claim> claims, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
