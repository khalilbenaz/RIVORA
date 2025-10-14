# RivoraApiClient

The `RivoraApiClient` is a typed HTTP client that provides a strongly-typed C# interface for consuming the RIVORA Framework API from other .NET applications.

## Installation

```bash
dotnet add package RVR.Framework.Client
```

## DI Registration

Register the client in your service collection:

```csharp
// In Program.cs
builder.Services.AddHttpClient<RivoraApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5220");
    client.DefaultRequestHeaders.Add("X-Tenant-Id", "my-tenant");
});
```

With custom configuration:

```csharp
builder.Services.AddHttpClient<RivoraApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RivoraApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthDelegatingHandler>()  // Auto-refresh tokens
.AddPolicyHandler(GetRetryPolicy());             // Polly retry policy
```

## Authentication

```csharp
public class MyService
{
    private readonly RivoraApiClient _client;

    public MyService(RivoraApiClient client)
    {
        _client = client;
    }

    public async Task AuthenticateAsync()
    {
        // Login and get tokens
        var authResponse = await _client.LoginAsync("admin", "Admin@123");

        // Set the Bearer token for subsequent requests
        _client.SetBearerToken(authResponse.AccessToken);

        // Refresh token when needed
        var refreshed = await _client.RefreshTokenAsync(authResponse.RefreshToken);
        _client.SetBearerToken(refreshed.AccessToken);
    }
}
```

## Products API

```csharp
public async Task ProductOperationsAsync()
{
    // List all products
    var products = await _client.GetProductsAsync();

    // Get a single product
    var product = await _client.GetProductByIdAsync(productId);

    // Create a product
    var newProduct = await _client.CreateProductAsync(new CreateProductRequest
    {
        Name = "Widget Pro",
        Description = "A premium widget",
        Price = 29.99m,
        IsActive = true
    });

    // Update a product
    var updated = await _client.UpdateProductAsync(newProduct.Id, new UpdateProductRequest
    {
        Name = "Widget Pro v2",
        Price = 39.99m
    });

    // Delete a product
    await _client.DeleteProductAsync(newProduct.Id);
}
```

## Users API

```csharp
// List all users
var users = await _client.GetUsersAsync();

// Get a specific user
var user = await _client.GetUserByIdAsync(userId);
```

## Health Check

```csharp
// Check if the API is healthy (liveness probe)
var isHealthy = await _client.IsHealthyAsync();
if (!isHealthy)
{
    _logger.LogWarning("RIVORA API is not responding");
}
```

## Auto-Refresh Token Handler

Create a delegating handler to automatically refresh expired tokens:

```csharp
public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly RivoraApiClient _client;
    private string _refreshToken = string.Empty;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(_refreshToken))
        {
            // Attempt token refresh
            var authResponse = await _client.RefreshTokenAsync(_refreshToken, ct);
            _client.SetBearerToken(authResponse.AccessToken);
            _refreshToken = authResponse.RefreshToken;

            // Retry the original request
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
            response = await base.SendAsync(request, ct);
        }

        return response;
    }
}
```

## Configuration via appsettings.json

```json
{
  "RivoraApi": {
    "BaseUrl": "https://api.example.com",
    "TenantId": "my-tenant",
    "TimeoutSeconds": 30,
    "RetryCount": 3
  }
}
```

## Usage in Worker Services / Background Jobs

```csharp
public class DataSyncWorker : BackgroundService
{
    private readonly RivoraApiClient _client;

    public DataSyncWorker(RivoraApiClient client)
    {
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _client.LoginAsync("service-account", "service-password");

        while (!ct.IsCancellationRequested)
        {
            var products = await _client.GetProductsAsync(ct);
            // Sync products to local cache/database
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
}
```
