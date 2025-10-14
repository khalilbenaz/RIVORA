namespace RVR.Framework.Data.Tests;

using RVR.Framework.Data.Abstractions;
using RVR.Framework.Data.PostgreSQL;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Tests for the <see cref="PostgreSQLDbProvider"/> class.
/// </summary>
public class PostgreSQLDbProviderTests
{
    [Fact]
    public void Constructor_WithValidOptions_CreatesProvider()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL,
            ConnectionString = "Host=localhost;Database=TestDb;Username=postgres;"
        };

        // Act
        var provider = new PostgreSQLDbProvider(options);

        // Assert
        provider.Should().NotBeNull();
        provider.DatabaseType.Should().Be(DatabaseType.PostgreSQL);
    }

    [Fact]
    public void Constructor_WithoutConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new PostgreSQLDbProvider(options));
    }

    [Fact]
    public void GetConnectionString_ReturnsConnectionString()
    {
        // Arrange
        var connectionString = "Host=localhost;Database=TestDb;Username=postgres;";
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL,
            ConnectionString = connectionString
        };
        var provider = new PostgreSQLDbProvider(options);

        // Act
        var result = provider.GetConnectionString();

        // Assert
        result.Should().Be(connectionString);
    }

    [Fact]
    public void GetProviderName_ReturnsNpgsqlProviderName()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL,
            ConnectionString = "Host=localhost;Database=TestDb;Username=postgres;"
        };
        var provider = new PostgreSQLDbProvider(options);

        // Act
        var result = provider.GetProviderName();

        // Assert
        result.Should().Be("Npgsql.EntityFrameworkCore.PostgreSQL");
    }

    [Fact]
    public void CreateConnection_CreatesNpgsqlConnection()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL,
            ConnectionString = "Host=localhost;Database=TestDb;Username=postgres;"
        };
        var provider = new PostgreSQLDbProvider(options);

        // Act
        var connection = provider.CreateConnection();

        // Assert
        connection.Should().NotBeNull();
        connection.GetType().Name.Should().Be("NpgsqlConnection");
    }

    [Fact]
    public void ConfigureDbContext_ConfiguresNpgsqlOptions()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.PostgreSQL,
            ConnectionString = "Host=localhost;Database=TestDb;Username=postgres;",
            CommandTimeout = 60,
            EnableRetryOnFailure = true,
            MaxRetryCount = 3
        };
        var provider = new PostgreSQLDbProvider(options);
        var builder = new DbContextOptionsBuilder();

        // Act
        var result = provider.ConfigureDbContext(builder, options);

        // Assert
        result.Should().NotBeNull();
    }
}
