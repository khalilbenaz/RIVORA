using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RVR.Framework.SaaS.Analytics.Models;

namespace RVR.Framework.SaaS.Analytics;

/// <summary>
/// Default implementation of <see cref="ICrossTenantAnalyticsService"/> that uses
/// in-memory seed data for demonstration purposes. Replace the backing store
/// with a persistent repository or <c>IDbContextFactory</c> for production use.
/// </summary>
public sealed class CrossTenantAnalyticsService : ICrossTenantAnalyticsService
{
    private readonly ILogger<CrossTenantAnalyticsService> _logger;
    private readonly ConcurrentDictionary<Guid, SeedTenant> _tenants;

    /// <summary>
    /// Initializes a new instance of <see cref="CrossTenantAnalyticsService"/>
    /// and populates the in-memory store with representative seed data.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public CrossTenantAnalyticsService(ILogger<CrossTenantAnalyticsService> logger)
    {
        _logger = logger;
        _tenants = GenerateSeedData();
    }

    /// <inheritdoc />
    public Task<PlatformOverview> GetPlatformOverviewAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogInformation("Generating platform overview");

        var all = _tenants.Values.ToList();
        var active = all.Count(t => t.IsActive);
        var inactive = all.Count - active;
        var mrr = all.Where(t => t.IsActive).Sum(t => t.MonthlyRevenue);
        var arpt = active > 0 ? mrr / active : 0m;
        var totalCalls = all.Sum(t => t.ApiCallCount);
        var totalStorage = all.Sum(t => t.StorageBytes);

        var overview = new PlatformOverview(
            TotalTenants: all.Count,
            ActiveTenants: active,
            InactiveTenants: inactive,
            MonthlyRecurringRevenue: mrr,
            AverageRevenuePerTenant: arpt,
            TotalApiCalls: totalCalls,
            TotalStorageBytes: totalStorage,
            GeneratedAt: DateTime.UtcNow);

        _logger.LogDebug(
            "Platform overview: {Total} tenants, {Active} active, MRR={MRR:C}",
            all.Count, active, mrr);

        return Task.FromResult(overview);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TenantMetrics>> GetTenantMetricsAsync(
        TenantMetricsFilter? filter = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogInformation("Querying tenant metrics with filter: {@Filter}", filter);

        filter ??= new TenantMetricsFilter();

        IEnumerable<SeedTenant> query = _tenants.Values;

        // Apply filters
        if (filter.IsActive.HasValue)
            query = query.Where(t => t.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Plan))
            query = query.Where(t => string.Equals(t.Plan, filter.Plan, StringComparison.OrdinalIgnoreCase));

        if (filter.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt > filter.CreatedAfter.Value);

        if (filter.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt < filter.CreatedBefore.Value);

        // Apply sorting
        query = filter.SortBy?.ToUpperInvariant() switch
        {
            "APICALLCOUNT" => filter.Descending
                ? query.OrderByDescending(t => t.ApiCallCount)
                : query.OrderBy(t => t.ApiCallCount),
            "MONTHLYREVENUE" => filter.Descending
                ? query.OrderByDescending(t => t.MonthlyRevenue)
                : query.OrderBy(t => t.MonthlyRevenue),
            "STORAGEBYTES" => filter.Descending
                ? query.OrderByDescending(t => t.StorageBytes)
                : query.OrderBy(t => t.StorageBytes),
            "USERCOUNT" => filter.Descending
                ? query.OrderByDescending(t => t.UserCount)
                : query.OrderBy(t => t.UserCount),
            "CREATEDAT" => filter.Descending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt),
            "TENANTNAME" => filter.Descending
                ? query.OrderByDescending(t => t.Name)
                : query.OrderBy(t => t.Name),
            _ => filter.Descending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);

        var results = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantMetrics(
                TenantId: t.TenantId,
                TenantName: t.Name,
                Plan: t.Plan,
                IsActive: t.IsActive,
                CreatedAt: t.CreatedAt,
                LastActivityAt: t.LastActivityAt,
                ApiCallCount: t.ApiCallCount,
                StorageBytes: t.StorageBytes,
                UserCount: t.UserCount,
                MonthlyRevenue: t.MonthlyRevenue))
            .ToList();

        _logger.LogDebug("Tenant metrics query returned {Count} results (page {Page})", results.Count, page);

        return Task.FromResult<IReadOnlyList<TenantMetrics>>(results);
    }

    /// <inheritdoc />
    public Task<RevenueReport> GetRevenueReportAsync(DateRange range, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(range);
        ct.ThrowIfCancellationRequested();
        _logger.LogInformation("Generating revenue report from {From} to {To}", range.From, range.To);

        var activeTenants = _tenants.Values.Where(t => t.IsActive).ToList();
        var totalRevenue = activeTenants.Sum(t => t.MonthlyRevenue);

        // Generate daily revenue time-series (simulated distribution)
        var days = (int)(range.To - range.From).TotalDays + 1;
        var dailyRevenue = new List<TimeSeriesPoint<decimal>>(days);
        var dailyBase = totalRevenue / Math.Max(days, 1);
        var rng = new Random(range.From.GetHashCode());

        for (var i = 0; i < days; i++)
        {
            var date = range.From.AddDays(i);
            var variance = dailyBase * (decimal)(rng.NextDouble() * 0.4 - 0.2); // +/- 20%
            dailyRevenue.Add(new TimeSeriesPoint<decimal>(date, Math.Round(dailyBase + variance, 2)));
        }

        // Revenue by plan
        var revenueByPlan = activeTenants
            .GroupBy(t => t.Plan, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.MonthlyRevenue));

        var report = new RevenueReport(range, totalRevenue, dailyRevenue, revenueByPlan);

        _logger.LogDebug("Revenue report: total={Total:C}, plans={PlanCount}", totalRevenue, revenueByPlan.Count);
        return Task.FromResult(report);
    }

    /// <inheritdoc />
    public Task<UsageReport> GetUsageReportAsync(DateRange range, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(range);
        ct.ThrowIfCancellationRequested();
        _logger.LogInformation("Generating usage report from {From} to {To}", range.From, range.To);

        var allTenants = _tenants.Values.ToList();
        var totalCalls = allTenants.Sum(t => t.ApiCallCount);

        // Generate daily API call time-series (simulated distribution)
        var days = (int)(range.To - range.From).TotalDays + 1;
        var dailyApiCalls = new List<TimeSeriesPoint<long>>(days);
        var dailyBase = totalCalls / Math.Max(days, 1);
        var rng = new Random(range.From.GetHashCode() ^ 0x5A5A);

        for (var i = 0; i < days; i++)
        {
            var date = range.From.AddDays(i);
            var variance = (long)(dailyBase * (rng.NextDouble() * 0.4 - 0.2)); // +/- 20%
            dailyApiCalls.Add(new TimeSeriesPoint<long>(date, dailyBase + variance));
        }

        // Top tenants by API calls
        var topTenants = allTenants
            .OrderByDescending(t => t.ApiCallCount)
            .Take(10)
            .Select(t => new TenantUsageSummary(t.TenantId, t.Name, t.ApiCallCount, t.StorageBytes))
            .ToList();

        var report = new UsageReport(range, totalCalls, dailyApiCalls, topTenants);

        _logger.LogDebug("Usage report: totalCalls={Total}, topTenants={TopCount}", totalCalls, topTenants.Count);
        return Task.FromResult(report);
    }

    // ---- Seed data ----

    private static ConcurrentDictionary<Guid, SeedTenant> GenerateSeedData()
    {
        var tenants = new ConcurrentDictionary<Guid, SeedTenant>();
        var rng = new Random(42);
        var plans = new[] { "free", "pro", "enterprise" };
        var planPrices = new Dictionary<string, decimal>
        {
            ["free"] = 0m,
            ["pro"] = 49.99m,
            ["enterprise"] = 299.99m
        };

        var names = new[]
        {
            "Acme Corp", "Globex Industries", "Initech Solutions", "Umbrella Inc",
            "Stark Enterprises", "Wayne Technologies", "Cyberdyne Systems",
            "Soylent Corp", "Wonka Industries", "Tyrell Corporation",
            "Massive Dynamic", "Oscorp Technologies", "Aperture Science",
            "Weyland-Yutani", "Abstergo Industries"
        };

        foreach (var name in names)
        {
            var id = Guid.NewGuid();
            var plan = plans[rng.Next(plans.Length)];
            var isActive = rng.NextDouble() > 0.15; // ~85% active
            var createdDaysAgo = rng.Next(30, 730);

            tenants[id] = new SeedTenant
            {
                TenantId = id,
                Name = name,
                Plan = plan,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow.AddDays(-createdDaysAgo),
                LastActivityAt = isActive ? DateTime.UtcNow.AddMinutes(-rng.Next(1, 10080)) : null,
                ApiCallCount = rng.Next(1_000, 5_000_000),
                StorageBytes = (long)rng.Next(10_000_000, int.MaxValue),
                UserCount = rng.Next(1, 500),
                MonthlyRevenue = isActive ? planPrices[plan] + rng.Next(0, 100) : 0m
            };
        }

        return tenants;
    }

    private sealed class SeedTenant
    {
        public Guid TenantId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Plan { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? LastActivityAt { get; init; }
        public long ApiCallCount { get; init; }
        public long StorageBytes { get; init; }
        public int UserCount { get; init; }
        public decimal MonthlyRevenue { get; init; }
    }
}
