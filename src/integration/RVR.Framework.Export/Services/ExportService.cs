namespace RVR.Framework.Export.Services;

/// <summary>
/// Default implementation of <see cref="IExportService"/> that delegates
/// to format-specific export services for CSV, Excel, and PDF generation.
/// </summary>
internal sealed class ExportService : IExportService
{
    private readonly CsvExportService _csvService = new();
    private readonly ExcelExportService _excelService = new();
    private readonly PdfExportService _pdfService = new();

    /// <inheritdoc />
    public byte[] ExportToCsv<T>(IEnumerable<T> data, ExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _csvService.Export(data, options ?? new ExportOptions());
    }

    /// <inheritdoc />
    public byte[] ExportToExcel<T>(IEnumerable<T> data, ExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _excelService.Export(data, options ?? new ExportOptions());
    }

    /// <inheritdoc />
    public byte[] ExportToPdf<T>(IEnumerable<T> data, ExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _pdfService.Export(data, options ?? new ExportOptions());
    }
}
