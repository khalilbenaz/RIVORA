namespace RVR.Framework.Data.Tests;

using RVR.Framework.Data.Abstractions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the <see cref="DatabaseOptions"/> class.
/// </summary>
public class DatabaseOptionsTests
{
    [Fact]
    public void Validate_WithValidOptions_ReturnsEmptyErrors()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;Database=TestDb;"
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithConnectionStringName_ReturnsEmptyErrors()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionStringName = "DefaultConnection"
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithoutConnectionStringOrName_ReturnsError()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionStringName = null
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Contain("ConnectionString or ConnectionStringName");
    }

    [Fact]
    public void Validate_WithNegativeMaxRetryCount_ReturnsError()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;",
            MaxRetryCount = -1
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("MaxRetryCount"));
    }

    [Fact]
    public void Validate_WithZeroCommandTimeout_ReturnsError()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;",
            CommandTimeout = 0
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.Should().Contain(e => e.Contains("CommandTimeout"));
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var options = new DatabaseOptions();

        // Assert
        options.ConnectionStringName.Should().Be("DefaultConnection");
        options.EnableRetryOnFailure.Should().BeTrue();
        options.MaxRetryCount.Should().Be(5);
        options.CommandTimeout.Should().Be(30);
        options.EnableSensitiveDataLogging.Should().BeFalse();
        options.EnableDetailedErrors.Should().BeFalse();
        options.AutoMigrate.Should().BeFalse();
        options.UseLazyLoadingProxies.Should().BeFalse();
        options.ProviderOptions.Should().BeEmpty();
    }
}
