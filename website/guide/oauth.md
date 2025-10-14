# OAuth2/OIDC Setup

This guide covers configuring external identity providers (Azure AD, Keycloak, Auth0) with the RIVORA Framework.

## Prerequisites

- RIVORA Framework API project
- An external identity provider account
- HTTPS configured for your application

## Azure AD Configuration

### 1. Register an Application in Azure Portal

1. Go to **Azure Portal** > **Azure Active Directory** > **App registrations**
2. Click **New registration**
3. Set the redirect URI to `https://localhost:5220/signin-oidc`
4. Note the **Application (client) ID** and **Directory (tenant) ID**
5. Under **Certificates & secrets**, create a new client secret

### 2. Configure appsettings.json

```json
{
  "Authentication": {
    "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "your-azure-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "CallbackPath": "/signin-oidc",
      "SignedOutCallbackPath": "/signout-callback-oidc"
    }
  }
}
```

### 3. Configure in Program.cs

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "AzureAD";
})
.AddJwtBearer(options =>
{
    // Existing JWT configuration for API tokens
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
})
.AddOpenIdConnect("AzureAD", options =>
{
    var azureAd = builder.Configuration.GetSection("Authentication:AzureAd");
    options.Authority = $"{azureAd["Instance"]}{azureAd["TenantId"]}/v2.0";
    options.ClientId = azureAd["ClientId"];
    options.ClientSecret = azureAd["ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.CallbackPath = azureAd["CallbackPath"];
    options.SignedOutCallbackPath = azureAd["SignedOutCallbackPath"];
});
```

## Keycloak Configuration

### 1. Create a Realm and Client

1. Open Keycloak Admin Console
2. Create a new realm (e.g., `rivora`)
3. Create a new client with **Client type: OpenID Connect**
4. Set the redirect URI to `https://localhost:5220/signin-oidc`
5. Enable **Client authentication** and note the client secret

### 2. Configure appsettings.json

```json
{
  "Authentication": {
    "Keycloak": {
      "Authority": "https://keycloak.example.com/realms/rivora",
      "ClientId": "rivora-api",
      "ClientSecret": "your-keycloak-client-secret",
      "RequireHttpsMetadata": true
    }
  }
}
```

### 3. Configure in Program.cs

```csharp
builder.Services.AddAuthentication()
    .AddOpenIdConnect("Keycloak", options =>
    {
        var keycloak = builder.Configuration.GetSection("Authentication:Keycloak");
        options.Authority = keycloak["Authority"];
        options.ClientId = keycloak["ClientId"];
        options.ClientSecret = keycloak["ClientSecret"];
        options.ResponseType = "code";
        options.RequireHttpsMetadata = bool.Parse(keycloak["RequireHttpsMetadata"] ?? "true");
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.GetClaimsFromUserInfoEndpoint = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
    });
```

## Auth0 Configuration

### 1. Create an Application in Auth0

1. Go to **Auth0 Dashboard** > **Applications**
2. Create a **Regular Web Application**
3. Set the callback URL to `https://localhost:5220/signin-oidc`
4. Note the **Domain**, **Client ID**, and **Client Secret**

### 2. Configure appsettings.json

```json
{
  "Authentication": {
    "Auth0": {
      "Domain": "your-tenant.auth0.com",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Audience": "https://api.rivora.com"
    }
  }
}
```

### 3. Configure in Program.cs

```csharp
builder.Services.AddAuthentication()
    .AddOpenIdConnect("Auth0", options =>
    {
        var auth0 = builder.Configuration.GetSection("Authentication:Auth0");
        options.Authority = $"https://{auth0["Domain"]}";
        options.ClientId = auth0["ClientId"];
        options.ClientSecret = auth0["ClientSecret"];
        options.ResponseType = "code";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                var logoutUri = $"https://{auth0["Domain"]}/v2/logout?client_id={auth0["ClientId"]}";
                context.Response.Redirect(logoutUri);
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
```

## Claims Transformation

Map external provider claims to RIVORA Framework claims:

```csharp
builder.Services.AddRvrClaimsTransformation(options =>
{
    // Map common claims
    options.MapClaim("preferred_username", ClaimTypes.Name);
    options.MapClaim("email", ClaimTypes.Email);
    options.MapClaim("given_name", ClaimTypes.GivenName);
    options.MapClaim("family_name", ClaimTypes.Surname);

    // Map provider-specific claims
    options.MapClaim("groups", "Role");          // Azure AD groups
    options.MapClaim("realm_access.roles", "Role"); // Keycloak roles
    options.MapClaim("https://app.example.com/roles", "Role"); // Auth0 custom claim

    // Map tenant claim
    options.MapClaim("tenant_id", "TenantId");
    options.MapClaim("org_id", "TenantId");      // Auth0 organizations
});
```

## External Login Flow

```csharp
[HttpGet("external-login/{provider}")]
[AllowAnonymous]
public IActionResult ExternalLogin(string provider, string returnUrl = "/")
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = Url.Action("ExternalLoginCallback", new { returnUrl }),
        Items = { { "scheme", provider } }
    };
    return Challenge(properties, provider);
}

[HttpGet("external-login-callback")]
[AllowAnonymous]
public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
{
    var result = await HttpContext.AuthenticateAsync();
    if (!result.Succeeded)
        return Unauthorized();

    var claims = result.Principal!.Claims;
    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

    // Find or create user in RIVORA Framework
    var user = await _userService.FindOrCreateFromExternalLoginAsync(email, name);

    // Issue local JWT
    var jwt = await _authService.GenerateTokenAsync(user);
    return Ok(jwt);
}
```

## Testing with Multiple Providers

```csharp
// In appsettings.Development.json
{
  "Authentication": {
    "AllowedProviders": ["AzureAD", "Keycloak", "Auth0"],
    "DefaultProvider": "AzureAD"
  }
}
```

Retrieve available providers at runtime:

```csharp
[HttpGet("providers")]
[AllowAnonymous]
public IActionResult GetProviders()
{
    var providers = _configuration.GetSection("Authentication:AllowedProviders")
        .Get<string[]>() ?? Array.Empty<string>();
    return Ok(providers);
}
```
