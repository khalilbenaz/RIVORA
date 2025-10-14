using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

namespace RVR.SaaS.Starter.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/users/login", new { email, password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    _http.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
                    ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
        }
        return false;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _http.DefaultRequestHeaders.Authorization = null;
        ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
    }

    public async Task<string?> GetTokenAsync()
        => await _localStorage.GetItemAsync<string>("authToken");
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
