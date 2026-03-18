namespace RVR.Framework.SaaS.Analytics.Models;

/// <summary>
/// Usage analytics report for a given date range, including daily API call volumes
/// and the top tenants ranked by consumption.
/// </summary>
/// <param name="Range">The date range this report covers.</param>
/// <param name="TotalApiCalls">Aggregate API calls across all tenants within the date range.</param>
/// <param name="DailyApiCalls">Daily API call time-series data points.</param>
/// <param name="TopTenants">Top tenants ranked by usage during the reporting period.</param>
public sealed record UsageReport(
    DateRange Range,
    long TotalApiCalls,
    List<TimeSeriesPoint<long>> DailyApiCalls,
    List<TenantUsageSummary> TopTenants);

/// <summary>
/// Summary of a single tenant's resource usage within a reporting period.
/// </summary>
/// <param name="TenantId">Unique identifier of the tenant.</param>
/// <param name="Name">Display name of the tenant.</param>
/// <param name="ApiCalls">Total API calls made by this tenant during the period.</param>
/// <param name="StorageBytes">Total storage consumed by this tenant in bytes.</param>
public sealed record TenantUsageSummary(
    Guid TenantId,
    string Name,
    long ApiCalls,
    long StorageBytes);

/// <summary>
/// Represents a date range with inclusive start and end boundaries.
/// </summary>
/// <param name="From">Start of the range (inclusive, UTC).</param>
/// <param name="To">End of the range (inclusive, UTC).</param>
public sealed record DateRange(DateTime From, DateTime To);

/// <summary>
/// A single data point in a time-series, pairing a date with a typed value.
/// </summary>
/// <typeparam name="T">The type of the measured value.</typeparam>
/// <param name="Date">The UTC date this data point represents.</param>
/// <param name="Value">The measured value on this date.</param>
public sealed record TimeSeriesPoint<T>(DateTime Date, T Value);
