using System.Text;
using FluentAssertions;
using RVR.Framework.Export;
using RVR.Framework.Export.Services;
using Xunit;

namespace RVR.Framework.Integration.Tests;

public class TestExportItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
}

public class ExportServiceTests
{
    private readonly ExportService _exportService = new();

    private static List<TestExportItem> CreateTestData() =>
    [
        new() { Id = 1, Name = "Widget A", Price = 19.99m, IsActive = true, CreatedAt = new DateTime(2025, 1, 15), Description = "A test widget" },
        new() { Id = 2, Name = "Widget B", Price = 29.50m, IsActive = false, CreatedAt = new DateTime(2025, 2, 20), Description = null },
        new() { Id = 3, Name = "Gadget C", Price = 99.00m, IsActive = true, CreatedAt = new DateTime(2025, 3, 10), Description = "Premium gadget" },
    ];

    // ── CSV tests ──────────────────────────────────────────────────

    [Fact]
    public void ExportToCsv_ProducesNonEmptyOutput()
    {
        var data = CreateTestData();

        var bytes = _exportService.ExportToCsv(data);

        bytes.Should().NotBeNullOrEmpty();
        var csv = Encoding.UTF8.GetString(bytes);
        csv.Should().Contain("Name");
        csv.Should().Contain("Widget A");
    }

    [Fact]
    public void ExportToCsv_FormulaInjectionProtection_PrefixesDangerousValues()
    {
        var data = new List<TestExportItem>
        {
            new() { Id = 1, Name = "=CMD()", Price = 10m, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "+SUM(A1:A2)", Price = 20m, IsActive = true, CreatedAt = DateTime.Now },
            new() { Id = 3, Name = "@RISK", Price = 30m, IsActive = true, CreatedAt = DateTime.Now },
        };

        var bytes = _exportService.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(bytes);

        // Values starting with =, +, @ should be prefixed with single quote for formula injection protection
        csv.Should().Contain("'=CMD()");
        csv.Should().Contain("'+SUM(A1:A2)");
        csv.Should().Contain("'@RISK");
    }

    [Fact]
    public void ExportToCsv_WithIncludeColumns_OnlyIncludesSpecifiedColumns()
    {
        var data = CreateTestData();
        var options = new ExportOptions { IncludeColumns = ["Name", "Price"] };

        var bytes = _exportService.ExportToCsv(data, options);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("Name");
        csv.Should().Contain("Price");
        csv.Should().NotContain("Description");
        csv.Should().NotContain("IsActive");
    }

    [Fact]
    public void ExportToCsv_WithExcludeColumns_ExcludesSpecifiedColumns()
    {
        var data = CreateTestData();
        var options = new ExportOptions { ExcludeColumns = ["Description", "IsActive"] };

        var bytes = _exportService.ExportToCsv(data, options);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("Name");
        csv.Should().Contain("Price");
        csv.Should().NotContain("Description");
    }

    [Fact]
    public void ExportToCsv_EmptyCollection_ProducesHeaderOnly()
    {
        var bytes = _exportService.ExportToCsv(new List<TestExportItem>());
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("Name");
        // Should only have the header line
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(1);
    }

    [Fact]
    public void ExportToCsv_NullData_ThrowsArgumentNullException()
    {
        var act = () => _exportService.ExportToCsv<TestExportItem>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Excel tests ────────────────────────────────────────────────

    [Fact]
    public void ExportToExcel_ProducesValidXlsxBytes()
    {
        var data = CreateTestData();

        var bytes = _exportService.ExportToExcel(data);

        bytes.Should().NotBeNullOrEmpty();
        // XLSX files start with PK (ZIP archive magic bytes)
        bytes[0].Should().Be(0x50); // 'P'
        bytes[1].Should().Be(0x4B); // 'K'
    }

    [Fact]
    public void ExportToExcel_WithIncludeColumns_ProducesOutput()
    {
        var data = CreateTestData();
        var options = new ExportOptions
        {
            IncludeColumns = ["Name", "Price"],
            SheetName = "Products"
        };

        var bytes = _exportService.ExportToExcel(data, options);

        bytes.Should().NotBeNullOrEmpty();
        bytes.Length.Should().BeGreaterThan(100); // A valid XLSX should be more than trivial
    }

    [Fact]
    public void ExportToExcel_EmptyCollection_ProducesValidOutput()
    {
        var bytes = _exportService.ExportToExcel(new List<TestExportItem>());

        bytes.Should().NotBeNullOrEmpty();
        bytes[0].Should().Be(0x50); // 'P'
    }

    // ── PDF tests ──────────────────────────────────────────────────

    [Fact]
    public void ExportToPdf_ProducesNonEmptyBytes()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var data = CreateTestData();

        var bytes = _exportService.ExportToPdf(data);

        bytes.Should().NotBeNullOrEmpty();
        // PDF files start with %PDF
        var header = Encoding.ASCII.GetString(bytes, 0, Math.Min(4, bytes.Length));
        header.Should().Be("%PDF");
    }

    [Fact]
    public void ExportToPdf_WithOptions_GeneratesBytes()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var data = CreateTestData();
        var options = new ExportOptions
        {
            Title = "Test Export",
            IncludeColumns = ["Name", "Price"]
        };

        var bytes = _exportService.ExportToPdf(data, options);

        bytes.Should().NotBeNullOrEmpty();
        bytes.Length.Should().BeGreaterThan(100);
    }

    // ── ExportOptions tests ────────────────────────────────────────

    [Fact]
    public void ExportOptions_DefaultValues_AreCorrect()
    {
        var options = new ExportOptions();

        options.Title.Should().Be("Export");
        options.SheetName.Should().Be("Data");
        options.DateFormat.Should().Be("yyyy-MM-dd HH:mm:ss");
        options.DecimalFormat.Should().Be("0.00");
        options.IncludeColumns.Should().BeNull();
        options.ExcludeColumns.Should().BeNull();
    }
}
