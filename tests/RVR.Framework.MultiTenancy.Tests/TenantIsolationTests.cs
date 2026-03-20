namespace RVR.Framework.MultiTenancy.Tests;

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

/// <summary>
/// Tests verifying multi-tenant data isolation at multiple layers.
/// These tests ensure that tenant boundaries cannot be bypassed.
/// </summary>
public class TenantIsolationTests
{
    private readonly Mock<ITenantStore> _tenantStoreMock;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantIsolationTests()
    {
        _tenantStoreMock = new Mock<ITenantStore>();
        _logger = NullLogger<TenantMiddleware>.Instance;
    }

    private static HttpContext CreateHttpContext(
        bool isAuthenticated = false,
        string? tenantIdClaim = null,
        string? tenantHeader = null,
        string host = "localhost",
        IServiceProvider? serviceProvider = null)
    {
        var context = new DefaultHttpContext();

        if (isAuthenticated)
        {
            var claims = new List<Claim>();
            if (tenantIdClaim != null)
            {
                claims.Add(new Claim(TenantMiddleware.TenantIdClaimType, tenantIdClaim));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        if (tenantHeader != null)
        {
            context.Request.Headers["X-Tenant-Id"] = tenantHeader;
        }

        context.Request.Host = new HostString(host);
        context.Response.Body = new MemoryStream();

        if (serviceProvider != null)
        {
            context.RequestServices = serviceProvider;
        }
        else
        {
            var services = new ServiceCollection();
            context.RequestServices = services.BuildServiceProvider();
        }

        return context;
    }

    private IServiceProvider BuildServiceProvider(ITenantStore? tenantStore = null)
    {
        var services = new ServiceCollection();
        if (tenantStore != null)
        {
            services.AddSingleton(tenantStore);
        }

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task TenantMiddleware_ShouldIgnoreHeaderForUnauthenticatedRequests()
    {
        // Arrange: unauthenticated request with X-Tenant-Id header
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var context = CreateHttpContext(
            isAuthenticated: false,
            tenantHeader: "tenant-a");

        // Act
        await middleware.InvokeAsync(context);

        // Assert: header should be ignored for unauthenticated requests;
        // tenant may still resolve from subdomain, but the header value is not used
        nextCalled.Should().BeTrue("the middleware should call next even without a resolved tenant");
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        // Since host is "localhost" (no subdomain), and header is ignored, no tenant should be set
        tenantInfo.Should().BeNull("unauthenticated requests should not resolve tenant from header");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldRejectMismatchedTenantClaim()
    {
        // Arrange: authenticated user with TenantId=A, header X-Tenant-Id=B
        _tenantStoreMock
            .Setup(s => s.GetTenantAsync("tenant-b"))
            .ReturnsAsync(new TenantInfo { Id = "tenant-b", Name = "Tenant B", Identifier = "tenant-b" });

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var sp = BuildServiceProvider(_tenantStoreMock.Object);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "tenant-a",
            tenantHeader: "tenant-b",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: should return 403 Forbidden
        nextCalled.Should().BeFalse("the middleware should short-circuit on tenant mismatch");
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task TenantMiddleware_ShouldAcceptMatchingTenantClaim()
    {
        // Arrange: authenticated user with TenantId=A, header X-Tenant-Id=A
        _tenantStoreMock
            .Setup(s => s.GetTenantAsync("tenant-a"))
            .ReturnsAsync(new TenantInfo { Id = "tenant-a", Name = "Tenant A", Identifier = "tenant-a" });

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var sp = BuildServiceProvider(_tenantStoreMock.Object);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "tenant-a",
            tenantHeader: "tenant-a",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: tenant resolved correctly and next called
        nextCalled.Should().BeTrue();
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        tenantInfo.Should().NotBeNull();
        tenantInfo!.Id.Should().Be("tenant-a");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldResolveTenantFromSubdomain()
    {
        // Arrange: request to tenant-a.example.com (unauthenticated, no header)
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var context = CreateHttpContext(
            isAuthenticated: false,
            host: "tenant-a.example.com");

        // Act
        await middleware.InvokeAsync(context);

        // Assert: tenant resolved from subdomain
        nextCalled.Should().BeTrue();
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        tenantInfo.Should().NotBeNull("subdomain should resolve as tenant identifier");
        tenantInfo!.Id.Should().Be("tenant-a");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldReturn403OnInvalidTenantId()
    {
        // Arrange: authenticated request with non-existent tenant ID
        _tenantStoreMock
            .Setup(s => s.GetTenantAsync("nonexistent"))
            .ReturnsAsync((TenantInfo?)null);

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var sp = BuildServiceProvider(_tenantStoreMock.Object);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantHeader: "nonexistent",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: 403 because tenant not found in store
        nextCalled.Should().BeFalse("the middleware should short-circuit when tenant is not found");
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task TenantMiddleware_ShouldContinueWithoutTenantWhenNoTenantSpecified()
    {
        // Arrange: authenticated request with no tenant header and no subdomain
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var context = CreateHttpContext(
            isAuthenticated: true,
            host: "localhost");

        // Act
        await middleware.InvokeAsync(context);

        // Assert: next called, no tenant set
        nextCalled.Should().BeTrue("middleware should pass through when no tenant is specified");
        context.Items.ContainsKey(TenantMiddleware.TenantKey).Should().BeFalse();
    }

    [Fact]
    public async Task TenantMiddleware_ShouldResolveFromQueryStringForAuthenticatedUser()
    {
        // Arrange: authenticated request with tenant in query string
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "tenant-q",
            host: "localhost");
        context.Request.QueryString = new QueryString("?tenant=tenant-q");

        // Act
        await middleware.InvokeAsync(context);

        // Assert: tenant resolved from query string
        nextCalled.Should().BeTrue();
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        tenantInfo.Should().NotBeNull();
        tenantInfo!.Id.Should().Be("tenant-q");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldFallbackToSubdomainWhenHeaderAbsentForAuthenticatedUser()
    {
        // Arrange: authenticated user, no header, but subdomain present
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        _tenantStoreMock
            .Setup(s => s.GetTenantAsync("acme"))
            .ReturnsAsync(new TenantInfo { Id = "acme", Name = "Acme Corp", Identifier = "acme" });

        var sp = BuildServiceProvider(_tenantStoreMock.Object);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "acme",
            host: "acme.app.example.com",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: tenant resolved from subdomain as fallback
        nextCalled.Should().BeTrue();
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        tenantInfo.Should().NotBeNull();
        tenantInfo!.Id.Should().Be("acme");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldPreferHeaderOverSubdomain()
    {
        // Arrange: authenticated user with both header and subdomain
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        _tenantStoreMock
            .Setup(s => s.GetTenantAsync("from-header"))
            .ReturnsAsync(new TenantInfo { Id = "from-header", Name = "Header Tenant", Identifier = "from-header" });

        var sp = BuildServiceProvider(_tenantStoreMock.Object);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "from-header",
            tenantHeader: "from-header",
            host: "from-subdomain.app.example.com",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: header takes precedence over subdomain
        nextCalled.Should().BeTrue();
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        tenantInfo.Should().NotBeNull();
        tenantInfo!.Id.Should().Be("from-header");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldWorkWithoutTenantStoreRegistered()
    {
        // Arrange: no ITenantStore registered (backward compatibility)
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var sp = BuildServiceProvider(tenantStore: null);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "basic-tenant",
            tenantHeader: "basic-tenant",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: tenant info created from ID directly
        nextCalled.Should().BeTrue();
        var tenantInfo = context.Items[TenantMiddleware.TenantKey] as TenantInfo;
        tenantInfo.Should().NotBeNull();
        tenantInfo!.Id.Should().Be("basic-tenant");
        tenantInfo.Name.Should().Be("basic-tenant");
    }

    [Fact]
    public async Task TenantMiddleware_ShouldRejectMismatchCaseInsensitively()
    {
        // Arrange: claim is "TENANT-A", header is "tenant-a" (should match case-insensitively)
        _tenantStoreMock
            .Setup(s => s.GetTenantAsync("tenant-a"))
            .ReturnsAsync(new TenantInfo { Id = "tenant-a", Name = "Tenant A", Identifier = "tenant-a" });

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TenantMiddleware(next, _logger);

        var sp = BuildServiceProvider(_tenantStoreMock.Object);
        var context = CreateHttpContext(
            isAuthenticated: true,
            tenantIdClaim: "TENANT-A",
            tenantHeader: "tenant-a",
            serviceProvider: sp);

        // Act
        await middleware.InvokeAsync(context);

        // Assert: case-insensitive comparison should allow this
        nextCalled.Should().BeTrue("tenant comparison should be case-insensitive");
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
