using RVR.Framework.SaaS.Analytics.Models;

namespace RVR.Framework.SaaS.Analytics;

/// <summary>
/// Provides cross-tenant analytics capabilities for platform administrators.
/// All operations require SuperAdmin authorization and aggregate data across
/// the entire tenant fleet.
/// </summary>
public interface ICrossTenantAnalyticsService
{
    /// <summary>
    /// Retrieves a high-level overview of the entire platform including tenant counts,
    /// revenue metrics, and aggregate resource usage.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A snapshot of the current platform state.</returns>
    Task<PlatformOverview> GetPlatformOverviewAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves per-tenant metrics with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="filter">Optional filter criteria for narrowing and ordering results.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of tenant metric records matching the filter.</returns>
    Task<IReadOnlyList<TenantMetrics>> GetTenantMetricsAsync(
        TenantMetricsFilter? filter = null,
        CancellationToken ct = default);

    /// <summary>
    /// Generates a revenue report for the specified date range, including daily breakdowns
    /// and revenue segmented by subscription plan.
    /// </summary>
    /// <param name="range">The date range to report on.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A revenue report with time-series and plan-level data.</returns>
    Task<RevenueReport> GetRevenueReportAsync(DateRange range, CancellationToken ct = default);

    /// <summary>
    /// Generates a usage report for the specified date range, including daily API call
    /// volumes and the top tenants by consumption.
    /// </summary>
    /// <param name="range">The date range to report on.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A usage report with time-series and top-tenant data.</returns>
    Task<UsageReport> GetUsageReportAsync(DateRange range, CancellationToken ct = default);
}
