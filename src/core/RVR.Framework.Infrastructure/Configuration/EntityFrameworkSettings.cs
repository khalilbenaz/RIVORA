namespace RVR.Framework.Infrastructure.Configuration;

/// <summary>
/// Configuration d'Entity Framework
/// </summary>
public class EntityFrameworkSettings
{
    public bool UseQuerySplitting { get; set; } = true;
    public string QuerySplittingBehavior { get; set; } = "SplitQuery";
    public bool EnableLazyLoading { get; set; } = false;
    public bool UseNoTracking { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public int CommandTimeout { get; set; } = 30;
}
