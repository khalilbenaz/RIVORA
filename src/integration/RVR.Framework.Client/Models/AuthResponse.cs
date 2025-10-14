namespace RVR.Framework.Client.Models;

/// <summary>
/// Authentication response containing token information.
/// </summary>
public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserName,
    string Email
);
