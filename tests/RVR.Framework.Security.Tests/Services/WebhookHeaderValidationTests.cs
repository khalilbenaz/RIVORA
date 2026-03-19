using FluentAssertions;

namespace RVR.Framework.Security.Tests.Services;

/// <summary>
/// Tests for the IsReservedHeader logic from WebhookSender.
/// Validates that reserved/sensitive headers are blocked to prevent
/// header injection attacks, while custom headers are allowed.
/// </summary>
public class WebhookHeaderValidationTests
{
    /// <summary>
    /// Reproduces the ReservedHeaders HashSet from WebhookSender.
    /// </summary>
    private static readonly HashSet<string> ReservedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Host", "Connection", "Content-Length", "Content-Type",
        "Transfer-Encoding", "Cookie", "Set-Cookie", "Proxy-Authorization",
        "X-Webhook-Id", "X-Webhook-Timestamp"
    };

    /// <summary>
    /// Reproduces the IsReservedHeader method from WebhookSender.
    /// </summary>
    private static bool IsReservedHeader(string headerName)
        => ReservedHeaders.Contains(headerName);

    #region Reserved Headers Should Be Blocked

    [Theory]
    [InlineData("Authorization")]
    [InlineData("Host")]
    [InlineData("Connection")]
    [InlineData("Content-Length")]
    [InlineData("Content-Type")]
    [InlineData("Transfer-Encoding")]
    [InlineData("Cookie")]
    [InlineData("Set-Cookie")]
    [InlineData("Proxy-Authorization")]
    [InlineData("X-Webhook-Id")]
    [InlineData("X-Webhook-Timestamp")]
    public void IsReservedHeader_ShouldReturnTrue_ForReservedHeaders(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue(
            $"'{headerName}' is a reserved header and must be blocked");
    }

    #endregion

    #region Case Insensitive Comparison

    [Theory]
    [InlineData("authorization")]
    [InlineData("AUTHORIZATION")]
    [InlineData("Authorization")]
    [InlineData("aUtHoRiZaTiOn")]
    public void IsReservedHeader_ShouldBeCaseInsensitive_ForAuthorization(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue(
            "header comparison must be case-insensitive to prevent bypass via casing");
    }

    [Theory]
    [InlineData("host")]
    [InlineData("HOST")]
    [InlineData("Host")]
    public void IsReservedHeader_ShouldBeCaseInsensitive_ForHost(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue();
    }

    [Theory]
    [InlineData("cookie")]
    [InlineData("COOKIE")]
    [InlineData("Cookie")]
    public void IsReservedHeader_ShouldBeCaseInsensitive_ForCookie(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue();
    }

    [Theory]
    [InlineData("content-type")]
    [InlineData("CONTENT-TYPE")]
    [InlineData("Content-Type")]
    [InlineData("content-TYPE")]
    public void IsReservedHeader_ShouldBeCaseInsensitive_ForContentType(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue();
    }

    [Theory]
    [InlineData("set-cookie")]
    [InlineData("SET-COOKIE")]
    [InlineData("Set-Cookie")]
    public void IsReservedHeader_ShouldBeCaseInsensitive_ForSetCookie(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue();
    }

    [Theory]
    [InlineData("proxy-authorization")]
    [InlineData("PROXY-AUTHORIZATION")]
    [InlineData("Proxy-Authorization")]
    public void IsReservedHeader_ShouldBeCaseInsensitive_ForProxyAuthorization(string headerName)
    {
        IsReservedHeader(headerName).Should().BeTrue();
    }

    #endregion

    #region Custom Headers Should Be Allowed

    [Theory]
    [InlineData("X-Custom")]
    [InlineData("X-Correlation-Id")]
    [InlineData("X-Request-Id")]
    [InlineData("X-Trace-Id")]
    [InlineData("X-Api-Version")]
    [InlineData("X-Tenant-Id")]
    public void IsReservedHeader_ShouldReturnFalse_ForCustomHeaders(string headerName)
    {
        IsReservedHeader(headerName).Should().BeFalse(
            $"'{headerName}' is a custom header and should be allowed");
    }

    [Theory]
    [InlineData("Accept")]
    [InlineData("Accept-Language")]
    [InlineData("Cache-Control")]
    [InlineData("User-Agent")]
    public void IsReservedHeader_ShouldReturnFalse_ForNonReservedStandardHeaders(string headerName)
    {
        IsReservedHeader(headerName).Should().BeFalse(
            $"'{headerName}' is not in the reserved set and should be allowed");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void IsReservedHeader_ShouldReturnFalse_ForEmptyString()
    {
        IsReservedHeader("").Should().BeFalse();
    }

    [Fact]
    public void IsReservedHeader_ShouldReturnFalse_ForWhitespace()
    {
        IsReservedHeader("   ").Should().BeFalse();
    }

    [Fact]
    public void IsReservedHeader_ShouldReturnFalse_ForPartialMatch()
    {
        // "Auth" is not "Authorization"
        IsReservedHeader("Auth").Should().BeFalse();
    }

    [Fact]
    public void IsReservedHeader_ShouldReturnFalse_ForSupersetMatch()
    {
        // "Authorization-Extra" is not "Authorization"
        IsReservedHeader("Authorization-Extra").Should().BeFalse();
    }

    [Fact]
    public void ReservedHeaders_ShouldContainExactlyElevenEntries()
    {
        ReservedHeaders.Should().HaveCount(11);
    }

    #endregion
}
