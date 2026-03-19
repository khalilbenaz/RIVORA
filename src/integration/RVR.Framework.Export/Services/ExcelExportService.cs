using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using ClosedXML.Excel;

namespace RVR.Framework.Export.Services;

/// <summary>
/// Handles exporting data to Excel (XLSX) format using ClosedXML.
/// </summary>
internal sealed class ExcelExportService
{
    /// <summary>
    /// Exports a collection of objects to an XLSX byte array.
    /// </summary>
    public byte[] Export<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> data, ExportOptions options)
    {
        var properties = PropertyHelper.GetFilteredProperties<T>(options);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(options.SheetName);

        // Write header row with styling
        for (var col = 0; col < properties.Length; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = properties[col].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x44, 0x72, 0xC4);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            cell.Style.Border.BottomBorderColor = XLColor.FromArgb(0x2F, 0x52, 0x96);
        }

        // Write data rows
        var dataList = data.ToList();
        for (var row = 0; row < dataList.Count; row++)
        {
            var item = dataList[row];
            for (var col = 0; col < properties.Length; col++)
            {
                var cell = worksheet.Cell(row + 2, col + 1);
                SetCellValue(cell, properties[col], item, options);

                // Alternate row shading for readability
                if (row % 2 == 1)
                {
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xF2, 0xF2, 0xF2);
                }
            }
        }

        // Auto-fit column widths
        worksheet.Columns().AdjustToContents();

        // Apply a minimum width so narrow columns remain readable
        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width < 10)
                column.Width = 10;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void SetCellValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IXLCell cell, PropertyInfo property, T item, ExportOptions options)
    {
        var value = property.GetValue(item);

        if (value is null)
        {
            cell.SetValue(Blank.Value);
            return;
        }

        switch (value)
        {
            case DateTime dt:
                cell.SetValue(dt);
                cell.Style.DateFormat.Format = options.DateFormat;
                break;
            case DateTimeOffset dto:
                cell.SetValue(dto.DateTime);
                cell.Style.DateFormat.Format = options.DateFormat;
                break;
            case DateOnly d:
                cell.SetValue(d.ToDateTime(TimeOnly.MinValue));
                cell.Style.DateFormat.Format = options.DateFormat;
                break;
            case decimal dec:
                cell.SetValue((double)dec);
                cell.Style.NumberFormat.Format = options.DecimalFormat;
                break;
            case double dbl:
                cell.SetValue(dbl);
                cell.Style.NumberFormat.Format = options.DecimalFormat;
                break;
            case float flt:
                cell.SetValue((double)flt);
                cell.Style.NumberFormat.Format = options.DecimalFormat;
                break;
            case int i:
                cell.SetValue(i);
                break;
            case long l:
                cell.SetValue(l);
                break;
            case bool b:
                cell.SetValue(b);
                break;
            default:
                cell.SetValue(value.ToString() ?? string.Empty);
                break;
        }
    }
}
