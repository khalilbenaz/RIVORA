namespace RVR.Framework.SaaS.Analytics.Models;

/// <summary>
/// Per-tenant metrics used for cross-tenant comparison and reporting.
/// </summary>
/// <param name="TenantId">Unique identifier of the tenant.</param>
/// <param name="TenantName">Display name of the tenant.</param>
/// <param name="Plan">Current subscription plan (e.g., "free", "pro", "enterprise").</param>
/// <param name="IsActive">Whether the tenant is currently active.</param>
/// <param name="CreatedAt">UTC timestamp when the tenant was provisioned.</param>
/// <param name="LastActivityAt">UTC timestamp of the tenant's most recent activity, or <c>null</c> if no activity has been recorded.</param>
/// <param name="ApiCallCount">Total API calls made by this tenant in the current billing period.</param>
/// <param name="StorageBytes">Total storage consumed by this tenant in bytes.</param>
/// <param name="UserCount">Number of users belonging to this tenant.</param>
/// <param name="MonthlyRevenue">Monthly revenue attributed to this tenant.</param>
public sealed record TenantMetrics(
    Guid TenantId,
    string TenantName,
    string Plan,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastActivityAt,
    long ApiCallCount,
    long StorageBytes,
    int UserCount,
    decimal MonthlyRevenue);

/// <summary>
/// Filter, sort, and pagination criteria for querying tenant metrics.
/// </summary>
/// <param name="IsActive">If set, filters tenants by their active/inactive state.</param>
/// <param name="Plan">If set, filters tenants by their subscription plan name.</param>
/// <param name="CreatedAfter">If set, includes only tenants created after this UTC date.</param>
/// <param name="CreatedBefore">If set, includes only tenants created before this UTC date.</param>
/// <param name="SortBy">Property name to sort results by (e.g., "ApiCallCount", "MonthlyRevenue").</param>
/// <param name="Descending">Whether to sort in descending order. Defaults to <c>true</c>.</param>
/// <param name="Page">One-based page number for pagination. Defaults to 1.</param>
/// <param name="PageSize">Number of records per page. Defaults to 50.</param>
public sealed record TenantMetricsFilter(
    bool? IsActive = null,
    string? Plan = null,
    DateTime? CreatedAfter = null,
    DateTime? CreatedBefore = null,
    string? SortBy = null,
    bool Descending = true,
    int Page = 1,
    int PageSize = 50);
