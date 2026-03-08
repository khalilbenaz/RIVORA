namespace KBA.Framework.Data.Tests;

using KBA.Framework.Data.Abstractions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the <see cref="DatabaseProviderFactory"/> class.
/// </summary>
public class DatabaseProviderFactoryTests
{
    [Fact]
    public void DetectDatabaseType_WithSqlServerConnectionString_ReturnsSqlServer()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";

        // Act
        var result = DatabaseProviderFactory.DetectDatabaseType(connectionString);

        // Assert
        result.Should().Be(DatabaseType.SqlServer);
    }

    [Fact]
    public void DetectDatabaseType_WithPostgresConnectionString_ReturnsPostgreSQL()
    {
        // Arrange
        var connectionString = "Host=localhost;Database=TestDb;Username=postgres;Password=password";

        // Act
        var result = DatabaseProviderFactory.DetectDatabaseType(connectionString);

        // Assert
        result.Should().Be(DatabaseType.PostgreSQL);
    }

    [Fact]
    public void DetectDatabaseType_WithMySqlConnectionString_ReturnsMySQL()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;Uid=user;Password=password;";

        // Act
        var result = DatabaseProviderFactory.DetectDatabaseType(connectionString);

        // Assert
        result.Should().Be(DatabaseType.MySQL);
    }

    [Fact]
    public void DetectDatabaseType_WithSqliteConnectionString_ReturnsSQLite()
    {
        // Arrange
        var connectionString = "Data Source=test.db";

        // Act
        var result = DatabaseProviderFactory.DetectDatabaseType(connectionString);

        // Assert
        result.Should().Be(DatabaseType.SQLite);
    }

    [Fact]
    public void DetectDatabaseType_WithSqliteInMemoryConnectionString_ReturnsSQLite()
    {
        // Arrange
        var connectionString = "Data Source=:memory:";

        // Act
        var result = DatabaseProviderFactory.DetectDatabaseType(connectionString);

        // Assert
        result.Should().Be(DatabaseType.SQLite);
    }

    [Fact]
    public void DetectDatabaseType_WithEmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var connectionString = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DatabaseProviderFactory.DetectDatabaseType(connectionString));
    }

    [Fact]
    public void DetectDatabaseType_WithNullConnectionString_ThrowsArgumentException()
    {
        // Arrange
        string? connectionString = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DatabaseProviderFactory.DetectDatabaseType(connectionString!));
    }

    [Fact]
    public void DetectDatabaseType_WithUnknownConnectionString_ReturnsSqlServer()
    {
        // Arrange
        var connectionString = "Unknown=connection;String=format";

        // Act
        var result = DatabaseProviderFactory.DetectDatabaseType(connectionString);

        // Assert
        result.Should().Be(DatabaseType.SqlServer);
    }
}
