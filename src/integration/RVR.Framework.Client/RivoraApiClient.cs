namespace RVR.Framework.Client;

using System.Net.Http.Json;
using System.Text.Json;
using RVR.Framework.Client.Models;

/// <summary>
/// Typed HTTP client for the RIVORA Framework API.
/// </summary>
public class RivoraApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of <see cref="RivoraApiClient"/>.
    /// </summary>
    /// <param name="httpClient">The underlying <see cref="HttpClient"/>.</param>
    public RivoraApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ──── Auth ────────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(string userName, string password, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login",
            new { UserName = userName, Password = password }, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions, ct))!;
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = refreshToken }, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions, ct))!;
    }

    /// <summary>
    /// Sets the Bearer token on the underlying HTTP client for subsequent requests.
    /// </summary>
    public void SetBearerToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // ──── Products ────────────────────────────────────────────────────

    /// <summary>
    /// Returns all products.
    /// </summary>
    public async Task<List<ProductResponse>> GetProductsAsync(CancellationToken ct = default)
    {
        return (await _httpClient.GetFromJsonAsync<List<ProductResponse>>("/api/products", _jsonOptions, ct))!;
    }

    /// <summary>
    /// Returns a single product by its identifier.
    /// </summary>
    public async Task<ProductResponse?> GetProductByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<ProductResponse?>($"/api/products/{id}", _jsonOptions, ct);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/products", request, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>(_jsonOptions, ct))!;
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public async Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/products/{id}", request, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>(_jsonOptions, ct))!;
    }

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/api/products/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ──── Users ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns all users.
    /// </summary>
    public async Task<List<UserResponse>> GetUsersAsync(CancellationToken ct = default)
    {
        return (await _httpClient.GetFromJsonAsync<List<UserResponse>>("/api/users", _jsonOptions, ct))!;
    }

    /// <summary>
    /// Returns a single user by their identifier.
    /// </summary>
    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<UserResponse?>($"/api/users/{id}", _jsonOptions, ct);
    }

    // ──── Health ──────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the API is healthy (liveness probe).
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health/live", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
