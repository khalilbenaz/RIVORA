namespace RVR.Framework.Security.OAuth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

/// <summary>
/// Extension methods for configuring OAuth 2.0 / OpenID Connect authentication
/// in the RIVORA Framework.
/// </summary>
public static class OAuthServiceCollectionExtensions
{
    /// <summary>
    /// The JWT Bearer authentication scheme name.
    /// Defined here to avoid a hard dependency on the JwtBearer package.
    /// </summary>
    public const string JwtBearerScheme = "Bearer";

    /// <summary>
    /// The authentication scheme name for Azure AD.
    /// </summary>
    public const string AzureAdScheme = "AzureAd";

    /// <summary>
    /// The authentication scheme name for Keycloak.
    /// </summary>
    public const string KeycloakScheme = "Keycloak";

    /// <summary>
    /// The authentication scheme name for Auth0.
    /// </summary>
    public const string Auth0Scheme = "Auth0";

    /// <summary>
    /// Adds OAuth 2.0 / OpenID Connect authentication to the service collection.
    /// Configures JWT Bearer as the default scheme and registers OIDC schemes
    /// for each provider whose options are supplied.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure the OAuth options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrOAuth(
        this IServiceCollection services,
        Action<OAuthOptions> configureOptions)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions is null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        var options = new OAuthOptions();
        configureOptions(options);

        services.AddOptions<OAuthOptions>()
            .Configure(configureOptions);

        // Determine which scheme is the default for challenges.
        var defaultChallengeScheme = ResolveDefaultChallengeScheme(options);

        var authBuilder = services.AddAuthentication(auth =>
        {
            // JWT Bearer remains the default for API authentication.
            // The host application is responsible for configuring the Bearer scheme
            // (e.g., via AddJwtBearer) before or after calling AddRvrOAuth.
            auth.DefaultAuthenticateScheme = JwtBearerScheme;
            auth.DefaultChallengeScheme = defaultChallengeScheme;
        });

        if (options.AzureAd is not null)
        {
            ConfigureAzureAd(authBuilder, options.AzureAd);
        }

        if (options.Keycloak is not null)
        {
            ConfigureKeycloak(authBuilder, options.Keycloak);
        }

        if (options.Auth0 is not null)
        {
            ConfigureAuth0(authBuilder, options.Auth0);
        }

        // Register the claims transformer so external claims are normalised.
        services.AddTransient<IClaimsTransformation, OAuthClaimsTransformer>();

        return services;
    }

    private static string ResolveDefaultChallengeScheme(OAuthOptions options)
    {
        return options.DefaultProvider switch
        {
            "AzureAd" when options.AzureAd is not null => AzureAdScheme,
            "Keycloak" when options.Keycloak is not null => KeycloakScheme,
            "Auth0" when options.Auth0 is not null => Auth0Scheme,
            _ => JwtBearerScheme
        };
    }

    private static void ConfigureAzureAd(AuthenticationBuilder builder, AzureAdOptions azureAd)
    {
        builder.AddOpenIdConnect(AzureAdScheme, "Azure AD", oidc =>
        {
            oidc.Authority = $"{azureAd.Instance.TrimEnd('/')}/{azureAd.TenantId}/v2.0";
            oidc.ClientId = azureAd.ClientId;
            oidc.ClientSecret = azureAd.ClientSecret;
            oidc.ResponseType = OpenIdConnectResponseType.Code;
            oidc.CallbackPath = azureAd.CallbackPath;
            oidc.SaveTokens = true;
            oidc.GetClaimsFromUserInfoEndpoint = true;

            oidc.Scope.Clear();
            oidc.Scope.Add("openid");
            oidc.Scope.Add("profile");
            oidc.Scope.Add("email");

            oidc.TokenValidationParameters.NameClaimType = "preferred_username";
            oidc.TokenValidationParameters.RoleClaimType = "roles";
        });
    }

    private static void ConfigureKeycloak(AuthenticationBuilder builder, KeycloakOptions keycloak)
    {
        var authority = !string.IsNullOrEmpty(keycloak.Authority)
            ? keycloak.Authority
            : $"{keycloak.Authority.TrimEnd('/')}/realms/{keycloak.Realm}";

        builder.AddOpenIdConnect(KeycloakScheme, "Keycloak", oidc =>
        {
            oidc.Authority = authority;
            oidc.ClientId = keycloak.ClientId;
            oidc.ClientSecret = keycloak.ClientSecret;
            oidc.ResponseType = OpenIdConnectResponseType.Code;
            oidc.CallbackPath = "/signin-keycloak";
            oidc.SaveTokens = true;
            oidc.GetClaimsFromUserInfoEndpoint = true;

            oidc.Scope.Clear();
            oidc.Scope.Add("openid");
            oidc.Scope.Add("profile");
            oidc.Scope.Add("email");
            oidc.Scope.Add("roles");

            oidc.TokenValidationParameters.NameClaimType = "preferred_username";
            oidc.TokenValidationParameters.RoleClaimType = "realm_roles";
        });
    }

    private static void ConfigureAuth0(AuthenticationBuilder builder, Auth0Options auth0)
    {
        builder.AddOpenIdConnect(Auth0Scheme, "Auth0", oidc =>
        {
            oidc.Authority = $"https://{auth0.Domain.TrimEnd('/')}";
            oidc.ClientId = auth0.ClientId;
            oidc.ClientSecret = auth0.ClientSecret;
            oidc.ResponseType = OpenIdConnectResponseType.Code;
            oidc.CallbackPath = "/signin-auth0";
            oidc.SaveTokens = true;
            oidc.GetClaimsFromUserInfoEndpoint = true;

            oidc.Scope.Clear();
            oidc.Scope.Add("openid");
            oidc.Scope.Add("profile");
            oidc.Scope.Add("email");

            if (!string.IsNullOrEmpty(auth0.Audience))
            {
                oidc.Resource = auth0.Audience;
            }

            oidc.TokenValidationParameters.NameClaimType = "nickname";
        });
    }
}
