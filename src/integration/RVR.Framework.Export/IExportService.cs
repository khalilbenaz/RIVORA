namespace RVR.Framework.Export;

/// <summary>
/// Provides data export capabilities to CSV, Excel, and PDF formats.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports the given data collection to CSV format.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="data">The data collection to export.</param>
    /// <param name="options">Optional export configuration.</param>
    /// <returns>A byte array containing the CSV file content.</returns>
    byte[] ExportToCsv<T>(IEnumerable<T> data, ExportOptions? options = null);

    /// <summary>
    /// Exports the given data collection to Excel (XLSX) format.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="data">The data collection to export.</param>
    /// <param name="options">Optional export configuration.</param>
    /// <returns>A byte array containing the XLSX file content.</returns>
    byte[] ExportToExcel<T>(IEnumerable<T> data, ExportOptions? options = null);

    /// <summary>
    /// Exports the given data collection to PDF format.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="data">The data collection to export.</param>
    /// <param name="options">Optional export configuration.</param>
    /// <returns>A byte array containing the PDF file content.</returns>
    byte[] ExportToPdf<T>(IEnumerable<T> data, ExportOptions? options = null);
}

/// <summary>
/// Configuration options for data export operations.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// The title displayed in the export document (used in PDF and Excel).
    /// </summary>
    public string Title { get; set; } = "Export";

    /// <summary>
    /// The worksheet name used in Excel exports.
    /// </summary>
    public string SheetName { get; set; } = "Data";

    /// <summary>
    /// When set, only these columns (property names) will be included in the export.
    /// </summary>
    public string[]? IncludeColumns { get; set; }

    /// <summary>
    /// When set, these columns (property names) will be excluded from the export.
    /// </summary>
    public string[]? ExcludeColumns { get; set; }

    /// <summary>
    /// The format string used for DateTime values.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// The format string used for decimal/floating-point values.
    /// </summary>
    public string DecimalFormat { get; set; } = "0.00";
}
