using RVR.Framework.Security.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace RVR.Framework.Security.Tests.Services;

/// <summary>
/// Tests pour PasswordHasher
/// </summary>
public class PasswordHasherTests
{
    private readonly Mock<ILogger<PasswordHasher>> _mockLogger;
    private readonly PasswordHasher _passwordHasher;
    private static readonly IOptions<PasswordHasherOptions> DefaultOptions =
        Options.Create(new PasswordHasherOptions());

    public PasswordHasherTests()
    {
        _mockLogger = new Mock<ILogger<PasswordHasher>>();
        _passwordHasher = new PasswordHasher(_mockLogger.Object, DefaultOptions);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PasswordHasher(null!, DefaultOptions));
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidLogger()
    {
        // Arrange
        var logger = new Mock<ILogger<PasswordHasher>>();

        // Act
        var hasher = new PasswordHasher(logger.Object, DefaultOptions);

        // Assert
        Assert.NotNull(hasher);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenWorkFactorTooLow()
    {
        // Arrange
        var logger = new Mock<ILogger<PasswordHasher>>();
        var options = Options.Create(new PasswordHasherOptions { WorkFactor = 2 });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PasswordHasher(logger.Object, options));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenWorkFactorTooHigh()
    {
        // Arrange
        var logger = new Mock<ILogger<PasswordHasher>>();
        var options = Options.Create(new PasswordHasherOptions { WorkFactor = 32 });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PasswordHasher(logger.Object, options));
    }

    [Fact]
    public void Constructor_ShouldAcceptCustomWorkFactor()
    {
        // Arrange
        var logger = new Mock<ILogger<PasswordHasher>>();
        var options = Options.Create(new PasswordHasherOptions { WorkFactor = 10 });

        // Act
        var hasher = new PasswordHasher(logger.Object, options);

        // Assert
        Assert.NotNull(hasher);
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ShouldReturnHash_WithValidPassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.StartsWith("$2", hash); // BCrypt hashes start with $2a$ or $2b$
    }

    [Fact]
    public void HashPassword_ShouldThrowArgumentException_WhenPasswordIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword(null!));
    }

    [Fact]
    public void HashPassword_ShouldThrowArgumentException_WhenPasswordIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword(string.Empty));
    }

    [Fact]
    public void HashPassword_ShouldThrowArgumentException_WhenPasswordIsWhitespace()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword("   "));
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Each hash should have a unique salt
    }

    [Fact]
    public void HashPassword_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var password = "P@$$w0rd!#%&*()[]{}";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var password = "Пароль123!中文密碼";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_ShouldHandleLongPasswords()
    {
        // Arrange
        var password = new string('a', 200);

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WithCorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WithIncorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsNull()
    {
        // Arrange
        var hash = _passwordHasher.HashPassword("SecurePassword123!");

        // Act
        var result = _passwordHasher.VerifyPassword(null!, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsEmpty()
    {
        // Arrange
        var hash = _passwordHasher.HashPassword("SecurePassword123!");

        // Act
        var result = _passwordHasher.VerifyPassword(string.Empty, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsWhitespace()
    {
        // Arrange
        var hash = _passwordHasher.HashPassword("SecurePassword123!");

        // Act
        var result = _passwordHasher.VerifyPassword("   ", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsNull()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var result = _passwordHasher.VerifyPassword(password, null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsEmpty()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var result = _passwordHasher.VerifyPassword(password, string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsWhitespace()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var result = _passwordHasher.VerifyPassword(password, "   ");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
    {
        // Arrange
        var password = "SecurePassword123!";
        var invalidHash = "not-a-valid-bcrypt-hash";

        // Act
        var result = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldBeCaseSensitive()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword("securepassword123!", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var password = "P@$$w0rd!#%&*()[]{}";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var password = "Пароль123!中文密碼";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldHandleLongPasswords()
    {
        // Arrange
        var password = new string('a', 200);
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void HashAndVerify_ShouldWorkTogether_WithMultiplePasswords()
    {
        // Arrange
        var passwords = new[]
        {
            "Password1!",
            "AnotherPassword2@",
            "ThirdPassword3#"
        };

        // Act & Assert
        foreach (var password in passwords)
        {
            var hash = _passwordHasher.HashPassword(password);
            var isValid = _passwordHasher.VerifyPassword(password, hash);
            Assert.True(isValid);
        }
    }

    [Fact]
    public void HashAndVerify_ShouldNotCrossValidate_BetweenDifferentPasswords()
    {
        // Arrange
        var password1 = "Password1!";
        var password2 = "Password2!";
        var hash1 = _passwordHasher.HashPassword(password1);
        var hash2 = _passwordHasher.HashPassword(password2);

        // Act & Assert
        Assert.False(_passwordHasher.VerifyPassword(password1, hash2));
        Assert.False(_passwordHasher.VerifyPassword(password2, hash1));
    }

    #endregion
}
