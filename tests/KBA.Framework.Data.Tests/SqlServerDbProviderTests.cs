namespace KBA.Framework.Data.Tests;

using KBA.Framework.Data.Abstractions;
using KBA.Framework.Data.SqlServer;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Tests for the <see cref="SqlServerDbProvider"/> class.
/// </summary>
public class SqlServerDbProviderTests
{
    [Fact]
    public void Constructor_WithValidOptions_CreatesProvider()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;Database=TestDb;"
        };

        // Act
        var provider = new SqlServerDbProvider(options);

        // Assert
        provider.Should().NotBeNull();
        provider.DatabaseType.Should().Be(DatabaseType.SqlServer);
    }

    [Fact]
    public void Constructor_WithoutConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new SqlServerDbProvider(options));
    }

    [Fact]
    public void GetConnectionString_ReturnsConnectionString()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;";
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = connectionString
        };
        var provider = new SqlServerDbProvider(options);

        // Act
        var result = provider.GetConnectionString();

        // Assert
        result.Should().Be(connectionString);
    }

    [Fact]
    public void GetProviderName_ReturnsSqlServerProviderName()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;Database=TestDb;"
        };
        var provider = new SqlServerDbProvider(options);

        // Act
        var result = provider.GetProviderName();

        // Assert
        result.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
    }

    [Fact]
    public void CreateConnection_CreatesSqlConnection()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;Database=TestDb;"
        };
        var provider = new SqlServerDbProvider(options);

        // Act
        var connection = provider.CreateConnection();

        // Assert
        connection.Should().NotBeNull();
        connection.GetType().Name.Should().Be("SqlConnection");
    }

    [Fact]
    public void ConfigureDbContext_ConfiguresSqlServerOptions()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SqlServer,
            ConnectionString = "Server=localhost;Database=TestDb;",
            CommandTimeout = 60,
            EnableRetryOnFailure = true,
            MaxRetryCount = 3
        };
        var provider = new SqlServerDbProvider(options);
        var builder = new DbContextOptionsBuilder();

        // Act
        var result = provider.ConfigureDbContext(builder, options);

        // Assert
        result.Should().NotBeNull();
    }
}
