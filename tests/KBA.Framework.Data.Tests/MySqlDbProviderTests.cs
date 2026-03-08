namespace KBA.Framework.Data.Tests;

using KBA.Framework.Data.Abstractions;
using KBA.Framework.Data.MySQL;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Tests for the <see cref="MySqlDbProvider"/> class.
/// </summary>
public class MySqlDbProviderTests
{
    [Fact]
    public void Constructor_WithValidOptions_CreatesProvider()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL,
            ConnectionString = "Server=localhost;Database=TestDb;Uid=user;Pwd=password;"
        };

        // Act
        var provider = new MySqlDbProvider(options);

        // Assert
        provider.Should().NotBeNull();
        provider.DatabaseType.Should().Be(DatabaseType.MySQL);
    }

    [Fact]
    public void Constructor_WithoutConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new MySqlDbProvider(options));
    }

    [Fact]
    public void GetConnectionString_ReturnsConnectionString()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;Uid=user;Pwd=password;";
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL,
            ConnectionString = connectionString
        };
        var provider = new MySqlDbProvider(options);

        // Act
        var result = provider.GetConnectionString();

        // Assert
        result.Should().Be(connectionString);
    }

    [Fact]
    public void GetProviderName_ReturnsMySqlProviderName()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL,
            ConnectionString = "Server=localhost;Database=TestDb;Uid=user;Pwd=password;"
        };
        var provider = new MySqlDbProvider(options);

        // Act
        var result = provider.GetProviderName();

        // Assert
        result.Should().Be("Pomelo.EntityFrameworkCore.MySql");
    }

    [Fact]
    public void CreateConnection_CreatesMySqlConnection()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL,
            ConnectionString = "Server=localhost;Database=TestDb;Uid=user;Pwd=password;"
        };
        var provider = new MySqlDbProvider(options);

        // Act
        var connection = provider.CreateConnection();

        // Assert
        connection.Should().NotBeNull();
        connection.GetType().Name.Should().Be("MySqlConnection");
    }

    [Fact]
    public void ConfigureDbContext_ConfiguresMySqlOptions()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            DatabaseType = DatabaseType.MySQL,
            ConnectionString = "Server=localhost;Database=TestDb;Uid=user;Pwd=password;",
            CommandTimeout = 60,
            EnableRetryOnFailure = true,
            MaxRetryCount = 3
        };
        var provider = new MySqlDbProvider(options);
        var builder = new DbContextOptionsBuilder();

        // Act
        var result = provider.ConfigureDbContext(builder, options);

        // Assert
        result.Should().NotBeNull();
    }
}
