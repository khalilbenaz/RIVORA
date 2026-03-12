using System;

namespace KBA.Framework.FeatureManagement.Domain;

/// <summary>
/// Represents a SaaS Edition (e.g., Basic, Pro, Enterprise)
/// </summary>
public class Edition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }

    /// <summary>
    /// Indicates if this is the default free edition assigned to new tenants.
    /// </summary>
    public bool IsFree { get; set; }
}