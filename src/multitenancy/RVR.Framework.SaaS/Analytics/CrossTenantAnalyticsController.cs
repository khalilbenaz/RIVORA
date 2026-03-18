using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;
using RVR.Framework.SaaS.Analytics.Models;

namespace RVR.Framework.SaaS.Analytics;

/// <summary>
/// REST API controller exposing cross-tenant analytics endpoints.
/// All endpoints require the <c>SuperAdmin</c> role.
/// </summary>
[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "SuperAdmin")]
public sealed class CrossTenantAnalyticsController : ControllerBase
{
    private readonly ICrossTenantAnalyticsService _analytics;
    private readonly ILogger<CrossTenantAnalyticsController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CrossTenantAnalyticsController"/>.
    /// </summary>
    /// <param name="analytics">The cross-tenant analytics service.</param>
    /// <param name="logger">Logger instance.</param>
    public CrossTenantAnalyticsController(
        ICrossTenantAnalyticsService analytics,
        ILogger<CrossTenantAnalyticsController> logger)
    {
        _analytics = analytics;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a high-level platform overview with tenant counts, revenue, and usage totals.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PlatformOverview"/> snapshot.</returns>
    /// <response code="200">Platform overview generated successfully.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller does not have the SuperAdmin role.</response>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(PlatformOverview), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlatformOverview>> GetOverview(CancellationToken ct)
    {
        _logger.LogInformation("SuperAdmin {User} requested platform overview", User.Identity?.Name);
        var overview = await _analytics.GetPlatformOverviewAsync(ct);
        return Ok(overview);
    }

    /// <summary>
    /// Retrieves per-tenant metrics with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="isActive">Filter by active/inactive state.</param>
    /// <param name="plan">Filter by subscription plan name.</param>
    /// <param name="createdAfter">Include tenants created after this date.</param>
    /// <param name="createdBefore">Include tenants created before this date.</param>
    /// <param name="sortBy">Property name to sort by (e.g., ApiCallCount, MonthlyRevenue).</param>
    /// <param name="descending">Sort in descending order (default: true).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 50, max: 200).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of <see cref="TenantMetrics"/> matching the filter.</returns>
    /// <response code="200">Tenant metrics returned successfully.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller does not have the SuperAdmin role.</response>
    [HttpGet("tenants")]
    [ProducesResponseType(typeof(IReadOnlyList<TenantMetrics>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<TenantMetrics>>> GetTenantMetrics(
        [FromQuery] bool? isActive,
        [FromQuery] string? plan,
        [FromQuery] DateTime? createdAfter,
        [FromQuery] DateTime? createdBefore,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var filter = new TenantMetricsFilter(
            IsActive: isActive,
            Plan: plan,
            CreatedAfter: createdAfter,
            CreatedBefore: createdBefore,
            SortBy: sortBy,
            Descending: descending,
            Page: page,
            PageSize: pageSize);

        _logger.LogInformation("SuperAdmin {User} requested tenant metrics with filter: {@Filter}", User.Identity?.Name, filter);
        var metrics = await _analytics.GetTenantMetricsAsync(filter, ct);
        return Ok(metrics);
    }

    /// <summary>
    /// Generates a revenue report for the specified date range.
    /// </summary>
    /// <param name="from">Start of the reporting period (UTC).</param>
    /// <param name="to">End of the reporting period (UTC).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="RevenueReport"/> covering the date range.</returns>
    /// <response code="200">Revenue report generated successfully.</response>
    /// <response code="400">Invalid or missing date range parameters.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller does not have the SuperAdmin role.</response>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RevenueReport>> GetRevenueReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        if (!from.HasValue || !to.HasValue)
            return BadRequest("Both 'from' and 'to' query parameters are required.");

        if (from.Value > to.Value)
            return BadRequest("'from' must be earlier than or equal to 'to'.");

        var range = new DateRange(from.Value, to.Value);

        _logger.LogInformation("SuperAdmin {User} requested revenue report: {From} to {To}", User.Identity?.Name, range.From, range.To);
        var report = await _analytics.GetRevenueReportAsync(range, ct);
        return Ok(report);
    }

    /// <summary>
    /// Generates a usage report for the specified date range.
    /// </summary>
    /// <param name="from">Start of the reporting period (UTC).</param>
    /// <param name="to">End of the reporting period (UTC).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="UsageReport"/> covering the date range.</returns>
    /// <response code="200">Usage report generated successfully.</response>
    /// <response code="400">Invalid or missing date range parameters.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller does not have the SuperAdmin role.</response>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(UsageReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UsageReport>> GetUsageReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        if (!from.HasValue || !to.HasValue)
            return BadRequest("Both 'from' and 'to' query parameters are required.");

        if (from.Value > to.Value)
            return BadRequest("'from' must be earlier than or equal to 'to'.");

        var range = new DateRange(from.Value, to.Value);

        _logger.LogInformation("SuperAdmin {User} requested usage report: {From} to {To}", User.Identity?.Name, range.From, range.To);
        var report = await _analytics.GetUsageReportAsync(range, ct);
        return Ok(report);
    }

    /// <summary>
    /// Exports tenant metrics as a CSV file. Optionally supports other formats via the
    /// <paramref name="format"/> query parameter.
    /// </summary>
    /// <param name="format">Export format. Currently supported: <c>csv</c> (default).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A downloadable file containing tenant metrics.</returns>
    /// <response code="200">Export file generated successfully.</response>
    /// <response code="400">Unsupported export format.</response>
    /// <response code="401">Caller is not authenticated.</response>
    /// <response code="403">Caller does not have the SuperAdmin role.</response>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        CancellationToken ct = default)
    {
        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest($"Unsupported export format: '{format}'. Supported formats: csv.");

        _logger.LogInformation("SuperAdmin {User} requested tenant metrics export (format={Format})", User.Identity?.Name, LogSanitizer.Sanitize(format));

        // Fetch all tenant metrics without pagination for the export
        var filter = new TenantMetricsFilter(Page: 1, PageSize: 200);
        var metrics = await _analytics.GetTenantMetricsAsync(filter, ct);

        var csv = GenerateCsv(metrics);
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
        var fileName = $"tenant-metrics-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static string GenerateCsv(IReadOnlyList<TenantMetrics> metrics)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("TenantId,TenantName,Plan,IsActive,CreatedAt,LastActivityAt,ApiCallCount,StorageBytes,UserCount,MonthlyRevenue");

        // Rows
        foreach (var m in metrics)
        {
            sb.Append(m.TenantId).Append(',');
            sb.Append(CsvEscape(m.TenantName)).Append(',');
            sb.Append(CsvEscape(m.Plan)).Append(',');
            sb.Append(m.IsActive).Append(',');
            sb.Append(m.CreatedAt.ToString("o", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(m.LastActivityAt?.ToString("o", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(m.ApiCallCount).Append(',');
            sb.Append(m.StorageBytes).Append(',');
            sb.Append(m.UserCount).Append(',');
            sb.AppendLine(m.MonthlyRevenue.ToString(CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
