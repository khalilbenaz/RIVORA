namespace KBA.Framework.Features.Dashboard.Pages;

using KBA.Framework.Features.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

/// <summary>
/// Page model for the feature flags dashboard.
/// Provides a UI for viewing and managing feature flags.
/// </summary>
public class FeaturesModel : PageModel
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<FeaturesModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeaturesModel"/> class.
    /// </summary>
    /// <param name="featureManager">The feature manager.</param>
    /// <param name="logger">The logger instance.</param>
    public FeaturesModel(IFeatureManager featureManager, ILogger<FeaturesModel> logger)
    {
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets or sets the list of features.
    /// </summary>
    public IList<FeatureInfo> Features { get; set; } = new List<FeatureInfo>();

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Handles GET requests to display the features dashboard.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Features = (await _featureManager.GetAllFeaturesAsync()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading features");
            StatusMessage = "Error loading features";
        }

        return Page();
    }

    /// <summary>
    /// Handles POST requests to toggle a feature.
    /// </summary>
    /// <param name="featureName">The name of the feature to toggle.</param>
    /// <param name="enabled">The new enabled state.</param>
    public async Task<IActionResult> OnPostToggleAsync(string featureName, bool enabled)
    {
        try
        {
            var result = await _featureManager.SetEnabledAsync(featureName, enabled);
            
            if (result)
            {
                StatusMessage = $"Feature '{featureName}' {(enabled ? "enabled" : "disabled")}";
            }
            else
            {
                StatusMessage = $"Failed to update feature '{featureName}'";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling feature {FeatureName}", featureName);
            StatusMessage = $"Error updating feature '{featureName}'";
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Handles POST requests to create a new feature.
    /// </summary>
    /// <param name="featureName">The name of the new feature.</param>
    /// <param name="description">The description of the new feature.</param>
    public async Task<IActionResult> OnPostCreateAsync(string featureName, string? description)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            StatusMessage = "Feature name is required";
            return RedirectToPage();
        }

        try
        {
            // Note: Creating new features requires a writable provider
            // This is a simplified example - in production you would use the Database or Azure provider
            StatusMessage = "Creating features requires a writable provider (Database or Azure)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature {FeatureName}", featureName);
            StatusMessage = $"Error creating feature '{featureName}'";
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Handles POST requests to delete a feature.
    /// </summary>
    /// <param name="featureName">The name of the feature to delete.</param>
    public async Task<IActionResult> OnPostDeleteAsync(string featureName)
    {
        try
        {
            // Note: Deleting features requires a writable provider
            StatusMessage = "Deleting features requires a writable provider (Database or Azure)";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature {FeatureName}", featureName);
            StatusMessage = $"Error deleting feature '{featureName}'";
        }

        return RedirectToPage();
    }
}
