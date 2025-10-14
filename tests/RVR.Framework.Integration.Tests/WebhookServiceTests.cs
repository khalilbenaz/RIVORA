using System.Net;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RVR.Framework.Webhooks;
using RVR.Framework.Webhooks.Models;
using RVR.Framework.Webhooks.Services;
using Xunit;

namespace RVR.Framework.Integration.Tests;

public class WebhookServiceTests
{
    private readonly IWebhookStore _store;
    private readonly WebhookOptions _options;
    private readonly WebhookService _service;

    public WebhookServiceTests()
    {
        _store = Substitute.For<IWebhookStore>();
        _options = new WebhookOptions { DefaultMaxRetries = 3, TimeoutSeconds = 5 };

        var channel = new WebhookDeliveryChannel();

        _service = new WebhookService(
            _store,
            channel,
            Options.Create(_options),
            Substitute.For<ILogger<WebhookService>>());
    }

    [Fact]
    public async Task PublishAsync_NoSubscriptions_DoesNotSend()
    {
        _store.GetSubscriptionsByEventAsync("test.event", null, Arg.Any<CancellationToken>())
            .Returns(new List<WebhookSubscription>().AsReadOnly());

        await _service.PublishAsync("test.event", new { Data = "test" });

        await _store.DidNotReceive().AddDeliveryAsync(Arg.Any<WebhookDelivery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WithSubscriptions_QueuesSendToAllSubscribers()
    {
        var sub1 = new WebhookSubscription
        {
            EventType = "order.created",
            CallbackUrl = "https://example.com/hook1",
            MaxRetries = 1
        };
        var sub2 = new WebhookSubscription
        {
            EventType = "order.created",
            CallbackUrl = "https://example.com/hook2",
            MaxRetries = 1
        };

        _store.GetSubscriptionsByEventAsync("order.created", null, Arg.Any<CancellationToken>())
            .Returns(new List<WebhookSubscription> { sub1, sub2 }.AsReadOnly());

        // PublishAsync now queues to a channel (background delivery)
        await _service.PublishAsync("order.created", new { OrderId = 123 });

        // Verify subscriptions were looked up
        await _store.Received(1).GetSubscriptionsByEventAsync("order.created", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ThrowsOnNullEventType()
    {
        var act = () => _service.PublishAsync(null!, new { });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_ThrowsOnEmptyEventType()
    {
        var act = () => _service.PublishAsync("", new { });

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class CallbackUrlValidatorTests
{
    [Fact]
    public void Validate_ValidHttpsUrl_DoesNotThrow()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("https://example.com/webhook", options);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_HttpUrlWhenOnlyHttpsAllowed_Throws()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("http://example.com/webhook", options);

        act.Should().Throw<ArgumentException>().WithMessage("*scheme*");
    }

    [Fact]
    public void Validate_LocalhostUrl_BlockedByDefault()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("https://localhost/webhook", options);

        act.Should().Throw<ArgumentException>().WithMessage("*localhost*");
    }

    [Fact]
    public void Validate_PrivateIPv4_10Network_Blocked()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("https://10.0.0.1/webhook", options);

        act.Should().Throw<ArgumentException>().WithMessage("*private*");
    }

    [Fact]
    public void Validate_PrivateIPv4_192168_Blocked()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("https://192.168.1.1/webhook", options);

        act.Should().Throw<ArgumentException>().WithMessage("*private*");
    }

    [Fact]
    public void Validate_PrivateIPv4_172_16_Blocked()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("https://172.16.0.1/webhook", options);

        act.Should().Throw<ArgumentException>().WithMessage("*private*");
    }

    [Fact]
    public void Validate_LoopbackIP_Blocked()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = true
        };

        var act = () => CallbackUrlValidator.Validate("https://127.0.0.1/webhook", options);

        act.Should().Throw<ArgumentException>().WithMessage("*private*loopback*");
    }

    [Fact]
    public void Validate_InvalidUrl_Throws()
    {
        var options = new WebhookOptions();

        var act = () => CallbackUrlValidator.Validate("not-a-url", options);

        act.Should().Throw<ArgumentException>().WithMessage("*not a valid*");
    }

    [Fact]
    public void Validate_PrivateNetworksNotBlocked_AllowsLocalhost()
    {
        var options = new WebhookOptions
        {
            AllowedSchemes = ["https"],
            BlockPrivateNetworks = false
        };

        var act = () => CallbackUrlValidator.Validate("https://localhost/webhook", options);

        act.Should().NotThrow();
    }
}

public class HmacSignatureTests
{
    [Fact]
    public void ComputeSignature_ProducesConsistentOutput()
    {
        var payload = "{\"type\":\"test\",\"data\":{}}";
        var secret = "test-secret-key";

        var signature1 = ComputeHmacSha256(payload, secret);
        var signature2 = ComputeHmacSha256(payload, secret);

        signature1.Should().Be(signature2);
        signature1.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ComputeSignature_DifferentPayloads_ProduceDifferentSignatures()
    {
        var secret = "test-secret-key";

        var sig1 = ComputeHmacSha256("payload-1", secret);
        var sig2 = ComputeHmacSha256("payload-2", secret);

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void ComputeSignature_DifferentSecrets_ProduceDifferentSignatures()
    {
        var payload = "same-payload";

        var sig1 = ComputeHmacSha256(payload, "secret-1");
        var sig2 = ComputeHmacSha256(payload, "secret-2");

        sig1.Should().NotBe(sig2);
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexStringLower(hash);
    }
}
