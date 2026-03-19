namespace RVR.Framework.SaaS.Analytics.Models;

/// <summary>
/// Revenue analytics report for a given date range, including daily time-series
/// data and revenue segmented by subscription plan.
/// </summary>
/// <param name="Range">The date range this report covers.</param>
/// <param name="TotalRevenue">Total revenue generated within the date range.</param>
/// <param name="DailyRevenue">Daily revenue time-series data points.</param>
/// <param name="RevenueByPlan">Revenue totals broken down by subscription plan name.</param>
public sealed record RevenueReport(
    DateRange Range,
    decimal TotalRevenue,
    List<TimeSeriesPoint<decimal>> DailyRevenue,
    Dictionary<string, decimal> RevenueByPlan);
