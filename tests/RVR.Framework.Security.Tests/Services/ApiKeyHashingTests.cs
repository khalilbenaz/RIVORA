using System.Security.Cryptography;
using System.Text;
using FluentAssertions;

namespace RVR.Framework.Security.Tests.Services;

/// <summary>
/// Tests for the SHA-256 hashing logic used in ApiKeyAuthenticationMiddleware.
/// The ComputeHash method is private, so we reproduce the same algorithm here
/// (SHA256.HashData + Convert.ToHexStringLower) to validate its properties.
/// </summary>
public class ApiKeyHashingTests
{
    /// <summary>
    /// Reproduces the ComputeHash logic from ApiKeyAuthenticationMiddleware.
    /// </summary>
    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    #region Consistency Tests

    [Fact]
    public void ComputeHash_ShouldProduceConsistentResult_ForSameInput()
    {
        // Arrange
        var input = "my-secret-api-key-12345";

        // Act
        var hash1 = ComputeHash(input);
        var hash2 = ComputeHash(input);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeHash_ShouldProduceConsistentResult_AcrossMultipleCalls()
    {
        // Arrange
        var input = "rvr-api-key-stable";

        // Act
        var hashes = Enumerable.Range(0, 100)
            .Select(_ => ComputeHash(input))
            .Distinct()
            .ToList();

        // Assert
        hashes.Should().ContainSingle("all 100 hashes of the same input must be identical");
    }

    #endregion

    #region Uniqueness Tests

    [Fact]
    public void ComputeHash_ShouldProduceDifferentHashes_ForDifferentInputs()
    {
        // Arrange
        var input1 = "api-key-alpha";
        var input2 = "api-key-beta";

        // Act
        var hash1 = ComputeHash(input1);
        var hash2 = ComputeHash(input2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void ComputeHash_ShouldBeCaseSensitive()
    {
        // Arrange
        var lower = "ApiKey";
        var upper = "APIKEY";

        // Act
        var hashLower = ComputeHash(lower);
        var hashUpper = ComputeHash(upper);

        // Assert
        hashLower.Should().NotBe(hashUpper);
    }

    [Theory]
    [InlineData("key1", "key2")]
    [InlineData("abc", "abd")]
    [InlineData("short", "shortx")]
    [InlineData("hello world", "hello_world")]
    public void ComputeHash_ShouldProduceDifferentHashes_ForVariousDistinctInputs(string a, string b)
    {
        // Act
        var hashA = ComputeHash(a);
        var hashB = ComputeHash(b);

        // Assert
        hashA.Should().NotBe(hashB);
    }

    #endregion

    #region Format Tests

    [Fact]
    public void ComputeHash_ShouldReturnLowercaseHexString()
    {
        // Arrange
        var input = "test-api-key";

        // Act
        var hash = ComputeHash(input);

        // Assert
        hash.Should().MatchRegex("^[0-9a-f]+$", "hash must be lowercase hexadecimal only");
    }

    [Fact]
    public void ComputeHash_ShouldReturn64Characters()
    {
        // SHA-256 produces 32 bytes = 64 hex characters
        var input = "any-input-value";

        // Act
        var hash = ComputeHash(input);

        // Assert
        hash.Should().HaveLength(64);
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    [InlineData("a-very-long-api-key-that-exceeds-typical-lengths-1234567890abcdefghij")]
    public void ComputeHash_ShouldAlwaysReturn64Characters_RegardlessOfInputLength(string input)
    {
        // Act
        var hash = ComputeHash(input);

        // Assert
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    #endregion

    #region Known-Value Tests

    [Fact]
    public void ComputeHash_ShouldMatchKnownSha256Value()
    {
        // The SHA-256 of "hello" is well-known
        var hash = ComputeHash("hello");

        hash.Should().Be("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824");
    }

    [Fact]
    public void ComputeHash_ShouldHandleEmptyString()
    {
        // SHA-256 of empty string is a known value
        var hash = ComputeHash("");

        hash.Should().Be("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
    }

    #endregion

    #region Special Character Tests

    [Fact]
    public void ComputeHash_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var input = "clef-api-\u00e9\u00e0\u00fc\u00f1";

        // Act
        var hash = ComputeHash(input);

        // Assert
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ComputeHash_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var input = "key!@#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        var hash = ComputeHash(input);

        // Assert
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    #endregion
}
