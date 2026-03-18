using Microsoft.Extensions.Logging;
using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS.Providers;

/// <summary>
/// Development-only SMS provider that logs messages to the console via <see cref="ILogger"/>.
/// No actual SMS is sent. Useful for local development and testing.
/// </summary>
public sealed class ConsoleSmsProvider : ISmsProvider
{
    private readonly ILogger<ConsoleSmsProvider> _logger;
    private int _messageCounter;

    /// <summary>
    /// Initializes a new instance of <see cref="ConsoleSmsProvider"/>.
    /// </summary>
    public ConsoleSmsProvider(ILogger<ConsoleSmsProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public SmsProvider ProviderType => SmsProvider.Console;

    /// <inheritdoc />
    public Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        var messageId = $"console-{Interlocked.Increment(ref _messageCounter):D6}";

        _logger.LogInformation(
            "=== Console SMS ===\n" +
            "  MessageId : {MessageId}\n" +
            "  To        : {To}\n" +
            "  From      : {From}\n" +
            "  Body      : {Body}\n" +
            "==================",
            messageId,
            message.To,
            message.From ?? "(default)",
            message.Body);

        return Task.FromResult(new SmsResult(true, MessageId: messageId, Provider: ProviderType));
    }

    /// <inheritdoc />
    public Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default)
    {
        _logger.LogDebug("Console SMS status query for {MessageId}: always returns Delivered", messageId);
        return Task.FromResult(new SmsStatus(messageId, SmsDeliveryStatus.Delivered, DateTime.UtcNow));
    }
}
