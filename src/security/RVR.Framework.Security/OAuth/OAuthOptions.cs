namespace RVR.Framework.Security.OAuth;

/// <summary>
/// Root configuration options for OAuth 2.0 / OpenID Connect integration.
/// Supports Azure AD, Keycloak, and Auth0 providers simultaneously.
/// </summary>
public class OAuthOptions
{
    /// <summary>
    /// Gets or sets the Azure AD provider options.
    /// </summary>
    public AzureAdOptions? AzureAd { get; set; }

    /// <summary>
    /// Gets or sets the Keycloak provider options.
    /// </summary>
    public KeycloakOptions? Keycloak { get; set; }

    /// <summary>
    /// Gets or sets the Auth0 provider options.
    /// </summary>
    public Auth0Options? Auth0 { get; set; }

    /// <summary>
    /// Gets or sets the default authentication provider name.
    /// Must match one of: "AzureAd", "Keycloak", "Auth0".
    /// </summary>
    public string DefaultProvider { get; set; } = "AzureAd";
}

/// <summary>
/// Configuration options for Azure Active Directory / Entra ID.
/// </summary>
public class AzureAdOptions
{
    /// <summary>
    /// Gets or sets the Azure AD instance URL.
    /// </summary>
    public string Instance { get; set; } = "https://login.microsoftonline.com/";

    /// <summary>
    /// Gets or sets the Azure AD tenant identifier.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application (client) identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for confidential client flows.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback path for the OIDC redirect.
    /// </summary>
    public string CallbackPath { get; set; } = "/signin-oidc";
}

/// <summary>
/// Configuration options for Keycloak identity provider.
/// </summary>
public class KeycloakOptions
{
    /// <summary>
    /// Gets or sets the Keycloak authority URL (e.g., https://keycloak.example.com/realms/{realm}).
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Keycloak realm name.
    /// </summary>
    public string Realm { get; set; } = string.Empty;
}

/// <summary>
/// Configuration options for Auth0 identity provider.
/// </summary>
public class Auth0Options
{
    /// <summary>
    /// Gets or sets the Auth0 domain (e.g., your-tenant.auth0.com).
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API audience identifier.
    /// </summary>
    public string Audience { get; set; } = string.Empty;
}
