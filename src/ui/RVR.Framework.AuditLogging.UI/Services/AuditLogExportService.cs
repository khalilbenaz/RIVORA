using RVR.Framework.AuditLogging.UI.Models;
using RVR.Framework.Export;

namespace RVR.Framework.AuditLogging.UI.Services;

/// <summary>
/// Service for exporting filtered audit log entries to CSV, Excel, or PDF using <see cref="IExportService"/>.
/// </summary>
public class AuditLogExportService
{
    private readonly IExportService _exportService;

    public AuditLogExportService(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Exports the given audit entries to the specified format.
    /// </summary>
    /// <param name="entries">The audit entries to export.</param>
    /// <param name="format">The export format: "csv", "excel", or "pdf".</param>
    /// <returns>A tuple of (byte[] fileContent, string contentType, string fileName).</returns>
    public (byte[] Content, string ContentType, string FileName) Export(
        IEnumerable<AuditEntryViewModel> entries,
        string format)
    {
        var options = new ExportOptions
        {
            Title = "Audit Log Export",
            SheetName = "AuditLogs",
            ExcludeColumns = new[] { "IsError", "IsSuccess", "UserDisplay" }
        };

        return format.ToLowerInvariant() switch
        {
            "csv" => (
                _exportService.ExportToCsv(entries, options),
                "text/csv",
                $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
            ),
            "excel" => (
                _exportService.ExportToExcel(entries, options),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx"
            ),
            "pdf" => (
                _exportService.ExportToPdf(entries, options),
                "application/pdf",
                $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf"
            ),
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
        };
    }
}
