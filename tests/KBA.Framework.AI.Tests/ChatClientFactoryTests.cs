using System;
using System.Collections.Generic;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace KBA.Framework.AI.Tests;

/// <summary>
/// Unit tests for the <see cref="ChatClientFactory"/> class.
/// </summary>
public class ChatClientFactoryTests
{
    private const string TestApiKey = "sk-test1234567890";

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ChatClientFactory(null!, "OpenAI"));
    }

    #endregion

    #region GetClient Tests

    [Fact]
    public void GetClient_WithNullProviderName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.GetClient(null!));
    }

    [Fact]
    public void GetClient_WithEmptyProviderName_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.GetClient(string.Empty));
    }

    [Fact]
    public void GetClient_WithUnregisteredProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act & Assert
        var act = () => factory.GetClient("UnknownProvider");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UnknownProvider*is not registered*");
    }

    [Fact]
    public void GetClient_WithRegisteredProvider_ReturnsClient()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act
        var client = factory.GetClient("OpenAI");

        // Assert
        client.Should().NotBeNull();
        client.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public void GetClient_ProviderNameIsCaseInsensitive()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act
        var client1 = factory.GetClient("OpenAI");
        var client2 = factory.GetClient("openai");
        var client3 = factory.GetClient("OPENAI");

        // Assert
        client1.Should().NotBeNull();
        client2.Should().NotBeNull();
        client3.Should().NotBeNull();
    }

    #endregion

    #region GetDefaultClient Tests

    [Fact]
    public void GetDefaultClient_ReturnsOpenAiClient()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act
        var client = factory.GetDefaultClient();

        // Assert
        client.Should().NotBeNull();
        client.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public void GetDefaultClient_WithCustomDefaultProvider_ReturnsCorrectClient()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI(defaultProvider: "OpenAI");

        // Act
        var client = factory.GetDefaultClient();

        // Assert
        client.Should().NotBeNull();
    }

    #endregion

    #region GetRegisteredProviders Tests

    [Fact]
    public void GetRegisteredProviders_ReturnsListOfProviders()
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act
        var providers = factory.GetRegisteredProviders();

        // Assert
        providers.Should().NotBeNull();
        providers.Should().Contain("openai");
    }

    #endregion

    #region IsProviderRegistered Tests

    [Theory]
    [InlineData("OpenAI", true)]
    [InlineData("openai", true)]
    [InlineData("OPENAI", true)]
    [InlineData("UnknownProvider", false)]
    [InlineData("", false)]
    public void IsProviderRegistered_ReturnsCorrectStatus(string providerName, bool expected)
    {
        // Arrange
        var factory = CreateFactoryWithOpenAI();

        // Act
        var result = factory.IsProviderRegistered(providerName);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region ServiceCollectionExtensions Tests

    [Fact]
    public void AddAIChatClients_RegistersFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAIChatClients(options =>
        {
            options.ApiKey = TestApiKey;
            options.ModelId = "gpt-4o";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var factory = serviceProvider.GetService<IChatClientFactory>();
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddAIChatClients_RegistersDefaultClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAIChatClients(options =>
        {
            options.ApiKey = TestApiKey;
            options.ModelId = "gpt-4o";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IChatClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenAiChatClient_RegistersOpenAiClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenAiChatClient(options =>
        {
            options.ApiKey = TestApiKey;
            options.ModelId = "gpt-4o";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetRequiredService<IChatClient>();
        client.Should().NotBeNull();
        client.ProviderName.Should().Be("OpenAI");
    }

    #endregion

    #region Helper Methods

    private static ChatClientFactory CreateFactoryWithOpenAI(string defaultProvider = "OpenAI")
    {
        var services = new ServiceCollection();

        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = TestApiKey;
            options.ModelId = "gpt-4o";
            options.IsEnabled = true;
        });

        services.AddHttpClient("OpenAI");
        services.AddSingleton<ILogger<OpenAiChatClient>>(_ => Substitute.For<ILogger<OpenAiChatClient>>());
        services.AddSingleton<ILogger<ChatClientBase>>(_ => Substitute.For<ILogger<ChatClientBase>>());

        var serviceProvider = services.BuildServiceProvider();

        return new ChatClientFactory(serviceProvider, defaultProvider);
    }

    #endregion
}
