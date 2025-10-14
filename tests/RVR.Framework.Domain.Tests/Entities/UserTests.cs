using RVR.Framework.Domain.Entities.Identity;

namespace RVR.Framework.Domain.Tests.Entities;

/// <summary>
/// Tests pour l'entité User
/// </summary>
public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUser_WithValidParameters()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userName = "testuser";
        var email = "test@example.com";

        // Act
        var user = new User(tenantId, userName, email);

        // Assert
        Assert.Equal(tenantId, user.TenantId);
        Assert.Equal(userName, user.UserName);
        Assert.Equal(userName.ToUpperInvariant(), user.NormalizedUserName);
        Assert.Equal(email, user.Email);
        Assert.Equal(email.ToUpperInvariant(), user.NormalizedEmail);
        Assert.True(user.IsActive);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenUserNameIsEmpty()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userName = "";
        var email = "test@example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new User(tenantId, userName, email));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenEmailIsEmpty()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userName = "testuser";
        var email = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new User(tenantId, userName, email));
    }

    [Fact]
    public void ConfirmEmail_ShouldSetEmailConfirmedToTrue()
    {
        // Arrange
        var user = new User(null, "testuser", "test@example.com");

        // Act
        user.ConfirmEmail();

        // Assert
        Assert.True(user.EmailConfirmed);
    }

    [Fact]
    public void EnableTwoFactor_ShouldSetTwoFactorEnabledToTrue()
    {
        // Arrange
        var user = new User(null, "testuser", "test@example.com");

        // Act
        user.EnableTwoFactor();

        // Assert
        Assert.True(user.TwoFactorEnabled);
    }

    [Fact]
    public void UpdatePersonalInfo_ShouldModifyUserProperties()
    {
        // Arrange
        var user = new User(null, "testuser", "test@example.com");
        var firstName = "John";
        var lastName = "Doe";
        var phoneNumber = "+1234567890";

        // Act
        user.UpdatePersonalInfo(firstName, lastName, phoneNumber);

        // Assert
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(phoneNumber, user.PhoneNumber);
        Assert.Equal("John Doe", user.FullName);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = new User(null, "testuser", "test@example.com");
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User(null, "testuser", "test@example.com");

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }
}
