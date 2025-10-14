# Module Export

**Package** : `RVR.Framework.Export`

## Description

Export de donnees en PDF (QuestPDF), Excel (ClosedXML) et CSV.

## Enregistrement

```csharp
builder.Services.AddRvrExport();
```

## Interfaces

```csharp
public interface IExportService
{
    byte[] ToPdf<T>(IEnumerable<T> data, ExportOptions? options = null);
    byte[] ToExcel<T>(IEnumerable<T> data, ExportOptions? options = null);
    byte[] ToCsv<T>(IEnumerable<T> data, ExportOptions? options = null);
}
```

## Utilisation rapide

```csharp
[HttpGet("export/{format}")]
public async Task<IActionResult> Export(string format)
{
    var data = await _mediator.Send(new GetProductsQuery());

    var (bytes, contentType, fileName) = format switch
    {
        "pdf" => (_export.ToPdf(data), "application/pdf", "products.pdf"),
        "excel" => (_export.ToExcel(data), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx"),
        "csv" => (_export.ToCsv(data), "text/csv", "products.csv"),
        _ => throw new ArgumentException($"Format '{format}' non supporte")
    };

    return File(bytes, contentType, fileName);
}
```

Voir le [guide Export](/guide/export) pour la personnalisation PDF avancee.
