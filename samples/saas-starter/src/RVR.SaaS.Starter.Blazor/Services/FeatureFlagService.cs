namespace RVR.SaaS.Starter.Blazor.Services;

public class FeatureFlagService
{
    private readonly HttpClient _http;
    private Dictionary<string, bool> _flags = new();

    public FeatureFlagService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync(Guid? tenantId = null)
    {
        try
        {
            var url = tenantId.HasValue 
                ? $"api/featureflags?tenantId={tenantId}" 
                : "api/featureflags";
            var flags = await _http.GetFromJsonAsync<List<FeatureFlagDto>>(url);
            if (flags != null)
            {
                _flags = flags.ToDictionary(f => f.Name, f => f.Enabled);
            }
        }
        catch
        {
            // Ignore errors, use default flags
        }
    }

    public bool IsEnabled(string name)
        => _flags.TryGetValue(name, out var enabled) && enabled;

    public async Task RefreshAsync(Guid? tenantId = null)
        => await InitializeAsync(tenantId);
}

public class FeatureFlagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
