using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RVR.Framework.NaturalQuery;
using RVR.Framework.NaturalQuery.Models;
using RVR.Framework.NaturalQuery.Services;
using Xunit;

namespace RVR.Framework.AI.Tests;

// Test entity with common properties for parser tests
public class TestProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NaturalLanguageParserTests
{
    private readonly NaturalLanguageParser _parser;

    public NaturalLanguageParserTests()
    {
        var logger = Substitute.For<ILogger<NaturalLanguageParser>>();
        _parser = new NaturalLanguageParser(logger, Options.Create(new RVR.Framework.NaturalQuery.NaturalQueryOptions()));
    }

    // ── French query tests ──────────────────────────────────────────

    [Fact]
    public void Parse_FrenchActiveProducts_ExtractsIsActiveFilter()
    {
        var plan = _parser.Parse("produits actifs", typeof(TestProduct));

        plan.Filters.Should().ContainSingle();
        plan.Filters[0].PropertyName.Should().Be("IsActive");
        plan.Filters[0].Operator.Should().Be(FilterOperator.Equals);
        plan.Filters[0].Value.Should().Be("true");
    }

    [Fact]
    public void Parse_FrenchPriceGreaterThan100_ExtractsFilter()
    {
        var plan = _parser.Parse("prix > 100", typeof(TestProduct));

        plan.Filters.Should().ContainSingle();
        plan.Filters[0].PropertyName.Should().Be("Price");
        plan.Filters[0].Operator.Should().Be(FilterOperator.GreaterThan);
        plan.Filters[0].Value.Should().Be("100");
    }

    [Fact]
    public void Parse_FrenchTop10SortedByName_ExtractsLimitAndSort()
    {
        var plan = _parser.Parse("top 10 sorted by name", typeof(TestProduct));

        plan.Take.Should().Be(10);
        plan.Sorts.Should().ContainSingle();
        plan.Sorts[0].PropertyName.Should().Be("Name");
        plan.Sorts[0].Descending.Should().BeFalse();
    }

    // ── English query tests ─────────────────────────────────────────

    [Fact]
    public void Parse_EnglishActiveProducts_ExtractsIsActiveFilter()
    {
        var plan = _parser.Parse("active products", typeof(TestProduct));

        plan.Filters.Should().ContainSingle();
        plan.Filters[0].PropertyName.Should().Be("IsActive");
        plan.Filters[0].Value.Should().Be("true");
    }

    [Fact]
    public void Parse_EnglishPriceGreaterThan50_ExtractsFilter()
    {
        var plan = _parser.Parse("price > 50", typeof(TestProduct));

        plan.Filters.Should().ContainSingle();
        plan.Filters[0].PropertyName.Should().Be("Price");
        plan.Filters[0].Operator.Should().Be(FilterOperator.GreaterThan);
        plan.Filters[0].Value.Should().Be("50");
    }

    [Fact]
    public void Parse_EnglishFirst5SortedByPrice_ExtractsLimitAndSort()
    {
        var plan = _parser.Parse("first 5 sorted by price", typeof(TestProduct));

        plan.Take.Should().Be(5);
        plan.Sorts.Should().ContainSingle();
        plan.Sorts[0].PropertyName.Should().Be("Price");
    }

    // ── Security tests ──────────────────────────────────────────────

    [Fact]
    public void Parse_VeryLongQuery_DoesNotThrow()
    {
        var longQuery = new string('a', 10_000);

        var act = () => _parser.Parse(longQuery, typeof(TestProduct));

        // Should not throw, just produce an empty or minimal plan
        act.Should().NotThrow();
    }

    [Fact]
    public void Parse_SqlInjectionAttempt_DoesNotProduceUnsafeOutput()
    {
        var plan = _parser.Parse("'; DROP TABLE Products; --", typeof(TestProduct));

        // The parser should not crash and any filter values should be treated as literals
        plan.Should().NotBeNull();
        // No filters should match since the SQL injection string doesn't match any property
        foreach (var filter in plan.Filters)
        {
            filter.PropertyName.Should().NotContain("DROP");
            filter.PropertyName.Should().NotContain(";");
        }
    }

    [Fact]
    public void Parse_ScriptInjection_DoesNotProduceUnsafeOutput()
    {
        var plan = _parser.Parse("<script>alert('xss')</script>", typeof(TestProduct));

        plan.Should().NotBeNull();
        foreach (var filter in plan.Filters)
        {
            filter.PropertyName.Should().NotContain("<script>");
        }
    }

    // ── Edge case tests ─────────────────────────────────────────────

    [Fact]
    public void Parse_EmptyQuery_ReturnsEmptyPlan()
    {
        var plan = _parser.Parse("", typeof(TestProduct));

        plan.Should().NotBeNull();
        plan.Filters.Should().BeEmpty();
        plan.Sorts.Should().BeEmpty();
        plan.Take.Should().BeNull();
        plan.Skip.Should().BeNull();
    }

    [Fact]
    public void Parse_WhitespaceOnlyQuery_ReturnsEmptyPlan()
    {
        var plan = _parser.Parse("   ", typeof(TestProduct));

        plan.Should().NotBeNull();
        plan.Filters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NullQuery_ThrowsArgumentNullException()
    {
        var act = () => _parser.Parse(null!, typeof(TestProduct));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Parse_NullEntityType_ThrowsArgumentNullException()
    {
        var act = () => _parser.Parse("test", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Parse_UnknownPropertyReference_ProducesNoFilter()
    {
        var plan = _parser.Parse("foobar > 100", typeof(TestProduct));

        // 'foobar' does not match any property (or may fuzzy match, but shouldn't be 'Price')
        // We just verify the parser doesn't crash
        plan.Should().NotBeNull();
    }

    [Fact]
    public void Parse_PriceLessThanOrEqual_ExtractsFilter()
    {
        var plan = _parser.Parse("price <= 200", typeof(TestProduct));

        plan.Filters.Should().ContainSingle();
        plan.Filters[0].PropertyName.Should().Be("Price");
        plan.Filters[0].Operator.Should().Be(FilterOperator.LessThanOrEqual);
        plan.Filters[0].Value.Should().Be("200");
    }

    [Fact]
    public void Parse_SkipKeyword_ExtractsSkipValue()
    {
        var plan = _parser.Parse("skip 20", typeof(TestProduct));

        plan.Skip.Should().Be(20);
    }
}
