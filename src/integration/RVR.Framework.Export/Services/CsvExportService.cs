using System.Globalization;
using System.Reflection;
using System.Text;

namespace RVR.Framework.Export.Services;

/// <summary>
/// Handles exporting data to CSV format using reflection-based property discovery.
/// </summary>
internal sealed class CsvExportService
{
    /// <summary>
    /// Exports a collection of objects to a CSV byte array.
    /// </summary>
    public byte[] Export<T>(IEnumerable<T> data, ExportOptions options)
    {
        var properties = PropertyHelper.GetFilteredProperties<T>(options);

        var sb = new StringBuilder();

        // Write header row
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        // Write data rows
        foreach (var item in data)
        {
            var values = properties.Select(p => FormatCsvValue(p, item, options));
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string FormatCsvValue<T>(PropertyInfo property, T item, ExportOptions options)
    {
        var value = property.GetValue(item);

        if (value is null)
            return string.Empty;

        var formatted = value switch
        {
            DateTime dt => dt.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            DateOnly d => d.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            decimal dec => dec.ToString(options.DecimalFormat, CultureInfo.InvariantCulture),
            double dbl => dbl.ToString(options.DecimalFormat, CultureInfo.InvariantCulture),
            float flt => flt.ToString(options.DecimalFormat, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        return EscapeCsvField(formatted);
    }

    private static string SanitizeFormulaInjection(string value)
    {
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            return "'" + value;
        }

        return value;
    }

    private static string EscapeCsvField(string field)
    {
        field = SanitizeFormulaInjection(field);

        if (field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
