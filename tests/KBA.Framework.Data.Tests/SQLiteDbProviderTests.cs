namespace KBA.Framework.Data.Tests;

using KBA.Framework.Data.Abstractions;
using KBA.Framework.Data.SQLite;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Tests for the <see cref="SQLiteDbProvider"/> class.
/// </summary>
public class SQLiteDbProviderTests
{
    [Fact]
    public void Constructor_WithValidOptions_CreatesProvider()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite,
            ConnectionString = "Data Source=test.db"
        };

        // Act
        var provider = new SQLiteDbProvider(options);

        // Assert
        provider.Should().NotBeNull();
        provider.DatabaseType.Should().Be(DatabaseType.SQLite);
    }

    [Fact]
    public void Constructor_WithoutConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new SQLiteDbProvider(options));
    }

    [Fact]
    public void GetConnectionString_ReturnsConnectionString()
    {
        // Arrange
        var connectionString = "Data Source=test.db";
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite,
            ConnectionString = connectionString
        };
        var provider = new SQLiteDbProvider(options);

        // Act
        var result = provider.GetConnectionString();

        // Assert
        result.Should().Be(connectionString);
    }

    [Fact]
    public void GetProviderName_ReturnsSqliteProviderName()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite,
            ConnectionString = "Data Source=test.db"
        };
        var provider = new SQLiteDbProvider(options);

        // Act
        var result = provider.GetProviderName();

        // Assert
        result.Should().Be("Microsoft.EntityFrameworkCore.Sqlite");
    }

    [Fact]
    public void CreateConnection_CreatesSqliteConnection()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite,
            ConnectionString = "Data Source=test.db"
        };
        var provider = new SQLiteDbProvider(options);

        // Act
        var connection = provider.CreateConnection();

        // Assert
        connection.Should().NotBeNull();
        connection.GetType().Name.Should().Be("SqliteConnection");
    }

    [Fact]
    public void ConfigureDbContext_ConfiguresSqliteOptions()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.SQLite,
            ConnectionString = "Data Source=test.db",
            CommandTimeout = 60
        };
        var provider = new SQLiteDbProvider(options);
        var builder = new DbContextOptionsBuilder();

        // Act
        var result = provider.ConfigureDbContext(builder, options);

        // Assert
        result.Should().NotBeNull();
    }
}
