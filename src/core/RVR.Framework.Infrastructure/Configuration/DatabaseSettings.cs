namespace RVR.Framework.Infrastructure.Configuration;

/// <summary>
/// Configuration de la base de données
/// </summary>
public class DatabaseSettings
{
    public string Provider { get; set; } = "SqlServer";
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
    public string MigrationsAssembly { get; set; } = "RVR.Framework.Infrastructure";
}
