using System.Globalization;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RVR.Framework.Export.Services;

/// <summary>
/// Handles exporting data to PDF format using QuestPDF.
/// </summary>
internal sealed class PdfExportService
{
    /// <summary>
    /// Exports a collection of objects to a PDF byte array.
    /// </summary>
    public byte[] Export<T>(IEnumerable<T> data, ExportOptions options)
    {
        var properties = PropertyHelper.GetFilteredProperties<T>(options);
        var dataList = data.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                // Header section with title
                page.Header().Column(column =>
                {
                    column.Item()
                        .PaddingBottom(10)
                        .Text(options.Title)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item()
                        .PaddingBottom(5)
                        .Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC | Records: {dataList.Count}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item()
                        .LineHorizontal(1)
                        .LineColor(Colors.Blue.Darken3);
                });

                // Content section with data table
                page.Content().PaddingTop(10).Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        for (var i = 0; i < properties.Length; i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    // Header row
                    table.Header(header =>
                    {
                        foreach (var property in properties)
                        {
                            header.Cell()
                                .Background(Colors.Blue.Darken3)
                                .Padding(5)
                                .Text(property.Name)
                                .FontColor(Colors.White)
                                .Bold()
                                .FontSize(8);
                        }
                    });

                    // Data rows
                    for (var row = 0; row < dataList.Count; row++)
                    {
                        var item = dataList[row];
                        var bgColor = row % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        foreach (var property in properties)
                        {
                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(4)
                                .Text(FormatValue(property, item, options))
                                .FontSize(8);
                        }
                    }
                });

                // Footer section with page number
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatValue<T>(PropertyInfo property, T item, ExportOptions options)
    {
        var value = property.GetValue(item);

        if (value is null)
            return string.Empty;

        return value switch
        {
            DateTime dt => dt.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            DateOnly d => d.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            decimal dec => dec.ToString(options.DecimalFormat, CultureInfo.InvariantCulture),
            double dbl => dbl.ToString(options.DecimalFormat, CultureInfo.InvariantCulture),
            float flt => flt.ToString(options.DecimalFormat, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }
}
