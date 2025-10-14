# Export PDF / Excel / CSV

## Formats supportes

| Format | Librairie | Usage |
|--------|-----------|-------|
| PDF | QuestPDF | Rapports, factures |
| Excel | ClosedXML | Exports de donnees |
| CSV | Built-in | Echanges simples |

## PDF avec QuestPDF

```csharp
public class InvoiceExporter : IPdfExporter<Invoice>
{
    public byte[] Export(Invoice invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Header().Text($"Facture #{invoice.Number}").Bold().FontSize(20);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1);
                    });
                    foreach (var line in invoice.Lines)
                    {
                        table.Cell().Text(line.Description);
                        table.Cell().Text(line.Quantity.ToString());
                        table.Cell().Text(line.Total.ToString("C"));
                    }
                });
            });
        }).GeneratePdf();
    }
}
```

## Excel avec ClosedXML

```csharp
public class ProductExcelExporter : IExcelExporter<Product>
{
    public byte[] Export(IEnumerable<Product> products)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");

        sheet.Cell(1, 1).Value = "Nom";
        sheet.Cell(1, 2).Value = "Prix";
        sheet.Cell(1, 3).Value = "Statut";

        var row = 2;
        foreach (var p in products)
        {
            sheet.Cell(row, 1).Value = p.Name;
            sheet.Cell(row, 2).Value = p.Price;
            sheet.Cell(row, 3).Value = p.Status.ToString();
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
```

## Endpoint d'export

```csharp
[HttpGet("export/{format}")]
public async Task<IActionResult> Export(string format)
{
    var products = await _mediator.Send(new GetAllProductsQuery());

    return format switch
    {
        "pdf" => File(_pdfExporter.Export(products), "application/pdf", "products.pdf"),
        "excel" => File(_excelExporter.Export(products), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx"),
        "csv" => File(_csvExporter.Export(products), "text/csv", "products.csv"),
        _ => BadRequest("Format non supporte")
    };
}
```
