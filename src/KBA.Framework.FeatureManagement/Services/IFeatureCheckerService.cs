using System.Threading.Tasks;

namespace KBA.Framework.FeatureManagement.Services;

/// <summary>
/// Core service to check if a feature is enabled or get its value based on the current tenant's edition.
/// </summary>
public interface IFeatureCheckerService
{
    /// <summary>
    /// Checks if a boolean feature is enabled for the current context/tenant.
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName);

    /// <summary>
    /// Gets the specific value (like a limit) of a feature.
    /// </summary>
    Task<string> GetValueAsync(string featureName);

    /// <summary>
    /// Gets the value as an integer (useful for limits like "MaxProjects").
    /// </summary>
    Task<int> GetValueAsIntAsync(string featureName);
}