namespace RVR.Framework.SaaS.Analytics.Models;

/// <summary>
/// A point-in-time snapshot of key platform-wide metrics aggregated across all tenants.
/// </summary>
/// <param name="TotalTenants">Total number of tenants on the platform.</param>
/// <param name="ActiveTenants">Number of tenants currently in an active state.</param>
/// <param name="InactiveTenants">Number of tenants that are suspended, deactivated, or otherwise inactive.</param>
/// <param name="MonthlyRecurringRevenue">Current monthly recurring revenue (MRR) in the platform's base currency.</param>
/// <param name="AverageRevenuePerTenant">Average revenue per active tenant (ARPT).</param>
/// <param name="TotalApiCalls">Aggregate API call count across all tenants for the current billing period.</param>
/// <param name="TotalStorageBytes">Aggregate storage consumption across all tenants in bytes.</param>
/// <param name="GeneratedAt">UTC timestamp indicating when this overview was generated.</param>
public sealed record PlatformOverview(
    int TotalTenants,
    int ActiveTenants,
    int InactiveTenants,
    decimal MonthlyRecurringRevenue,
    decimal AverageRevenuePerTenant,
    long TotalApiCalls,
    long TotalStorageBytes,
    DateTime GeneratedAt);
