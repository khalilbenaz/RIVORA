using System.Text.RegularExpressions;
using FluentAssertions;

namespace RVR.Framework.Security.Tests.Validation;

/// <summary>
/// Tests for the SafeNamePattern regex from StudioEndpoints.
/// This regex prevents path traversal attacks by validating solution names.
/// Pattern: ^[a-zA-Z][a-zA-Z0-9._-]{0,127}$
/// </summary>
public class PathTraversalValidationTests
{
    /// <summary>
    /// Reproduces the SafeNamePattern regex from StudioEndpoints.
    /// </summary>
    private static readonly Regex SafeNamePattern = new(@"^[a-zA-Z][a-zA-Z0-9._-]{0,127}$", RegexOptions.Compiled);

    #region Valid Names

    [Theory]
    [InlineData("MyApp")]
    [InlineData("myapp")]
    [InlineData("MYAPP")]
    public void SafeNamePattern_ShouldAcceptAlphabeticNames(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptHyphenatedName()
    {
        SafeNamePattern.IsMatch("my-app").Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptDottedName()
    {
        SafeNamePattern.IsMatch("App.Web").Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptUnderscoreName()
    {
        SafeNamePattern.IsMatch("test_project").Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptMixedValidCharacters()
    {
        SafeNamePattern.IsMatch("My-App.v2_release").Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptSingleLetter()
    {
        SafeNamePattern.IsMatch("A").Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptNameWithTrailingDigits()
    {
        SafeNamePattern.IsMatch("MyApp123").Should().BeTrue();
    }

    [Fact]
    public void SafeNamePattern_ShouldAcceptMaxLengthName()
    {
        // 1 letter + 127 chars = 128 total (max allowed)
        var name = "A" + new string('b', 127);
        SafeNamePattern.IsMatch(name).Should().BeTrue();
    }

    #endregion

    #region Path Traversal Attacks

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("../../secret")]
    [InlineData("..")]
    [InlineData("../")]
    [InlineData("foo/../bar")]
    public void SafeNamePattern_ShouldRejectPathTraversalAttempts(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "path traversal sequences must be blocked to prevent directory escape");
    }

    [Theory]
    [InlineData("..\\windows\\system32")]
    [InlineData("foo\\..\\bar")]
    public void SafeNamePattern_ShouldRejectWindowsPathTraversal(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "backslash-based path traversal must also be blocked");
    }

    #endregion

    #region Invalid Names - Empty and Whitespace

    [Fact]
    public void SafeNamePattern_ShouldRejectEmptyString()
    {
        SafeNamePattern.IsMatch("").Should().BeFalse();
    }

    [Fact]
    public void SafeNamePattern_ShouldRejectWhitespaceOnly()
    {
        SafeNamePattern.IsMatch("   ").Should().BeFalse();
    }

    [Fact]
    public void SafeNamePattern_ShouldRejectNameWithSpaces()
    {
        SafeNamePattern.IsMatch("my app").Should().BeFalse();
    }

    #endregion

    #region Invalid Names - Must Start With Letter

    [Theory]
    [InlineData("123start")]
    [InlineData("1App")]
    [InlineData("0test")]
    public void SafeNamePattern_ShouldRejectNamesStartingWithDigit(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "names must start with a letter");
    }

    [Theory]
    [InlineData("-my-app")]
    [InlineData("_my_app")]
    [InlineData(".hidden")]
    public void SafeNamePattern_ShouldRejectNamesStartingWithNonLetter(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "names must start with a letter, not a symbol");
    }

    #endregion

    #region Invalid Names - Special Characters

    [Theory]
    [InlineData("my app!")]
    [InlineData("app@host")]
    [InlineData("name#1")]
    [InlineData("test$var")]
    [InlineData("my%app")]
    [InlineData("app;drop")]
    [InlineData("name|pipe")]
    [InlineData("app`cmd`")]
    public void SafeNamePattern_ShouldRejectNamesWithSpecialCharacters(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "special characters that could enable injection must be blocked");
    }

    [Theory]
    [InlineData("app/sub")]
    [InlineData("app\\sub")]
    public void SafeNamePattern_ShouldRejectNamesWithPathSeparators(string name)
    {
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "path separators must be blocked to prevent directory traversal");
    }

    #endregion

    #region Invalid Names - Length Violations

    [Fact]
    public void SafeNamePattern_ShouldRejectNameExceedingMaxLength()
    {
        // 1 letter + 128 chars = 129 total (exceeds 128 max)
        var name = "A" + new string('b', 128);
        SafeNamePattern.IsMatch(name).Should().BeFalse(
            "names longer than 128 characters must be rejected");
    }

    #endregion
}
