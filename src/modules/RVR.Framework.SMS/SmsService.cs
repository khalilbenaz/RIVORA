using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.SMS.Configuration;
using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS;

/// <summary>
/// Default <see cref="ISmsService"/> implementation that routes SMS messages
/// to the configured provider and applies retry logic with exponential backoff.
/// </summary>
public sealed class SmsService : ISmsService
{
    private readonly ISmsProvider _provider;
    private readonly SmsOptions _options;
    private readonly ILogger<SmsService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SmsService"/>.
    /// </summary>
    /// <param name="provider">The resolved SMS provider.</param>
    /// <param name="options">SMS configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public SmsService(
        ISmsProvider provider,
        IOptions<SmsOptions> options,
        ILogger<SmsService> logger)
    {
        _provider = provider;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var effectiveMessage = message with
        {
            From = message.From ?? _options.DefaultFrom
        };

        return await ExecuteWithRetryAsync(
            () => _provider.SendAsync(effectiveMessage, ct),
            $"Send SMS to {message.To}",
            ct);
    }

    /// <inheritdoc />
    public async Task<SmsResult> SendBulkAsync(IEnumerable<SmsMessage> messages, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            return new SmsResult(true, Provider: _provider.ProviderType);
        }

        _logger.LogInformation("Starting bulk SMS send of {Count} messages via {Provider}", messageList.Count, _provider.ProviderType);

        var errors = new List<string>();
        var successCount = 0;
        string? lastMessageId = null;

        foreach (var message in messageList)
        {
            ct.ThrowIfCancellationRequested();

            var result = await SendAsync(message, ct);
            if (result.Success)
            {
                successCount++;
                lastMessageId = result.MessageId;
            }
            else
            {
                errors.Add($"{message.To}: {result.Error}");
            }
        }

        _logger.LogInformation("Bulk SMS complete. Sent={Sent}, Failed={Failed}, Total={Total}",
            successCount, errors.Count, messageList.Count);

        if (errors.Count == 0)
        {
            return new SmsResult(true, MessageId: lastMessageId, Provider: _provider.ProviderType);
        }

        if (successCount == 0)
        {
            return new SmsResult(false,
                Error: $"All {messageList.Count} messages failed. First error: {errors[0]}",
                Provider: _provider.ProviderType);
        }

        return new SmsResult(true,
            MessageId: lastMessageId,
            Error: $"{errors.Count}/{messageList.Count} messages failed. First error: {errors[0]}",
            Provider: _provider.ProviderType);
    }

    /// <inheritdoc />
    public async Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        return await _provider.GetStatusAsync(messageId, ct);
    }

    /// <summary>
    /// Executes an operation with exponential backoff retry for transient failures.
    /// </summary>
    private async Task<SmsResult> ExecuteWithRetryAsync(
        Func<Task<SmsResult>> operation,
        string operationName,
        CancellationToken ct)
    {
        var maxRetries = _options.MaxRetries;
        var baseDelayMs = _options.RetryBaseDelayMs;
        SmsResult? lastResult = null;

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            if (attempt > 0)
            {
                var delayMs = baseDelayMs * (1 << (attempt - 1)); // Exponential backoff
                _logger.LogWarning(
                    "Retrying '{Operation}' (attempt {Attempt}/{MaxRetries}) after {Delay}ms. Previous error: {Error}",
                    operationName, attempt, maxRetries, delayMs, lastResult?.Error);
                await Task.Delay(delayMs, ct);
            }

            try
            {
                lastResult = await operation();

                if (lastResult.Success)
                    return lastResult;

                // Non-retryable client errors (e.g., invalid phone number) should not be retried
                if (IsNonRetryableError(lastResult.Error))
                {
                    _logger.LogWarning("Non-retryable error for '{Operation}': {Error}", operationName, lastResult.Error);
                    return lastResult;
                }
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                // Timeout, not user cancellation
                _logger.LogWarning(ex, "Timeout during '{Operation}' (attempt {Attempt} of {MaxRetries})", operationName, attempt, maxRetries);
                lastResult = new SmsResult(false, Error: "Request timed out", Provider: _provider.ProviderType);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP error during '{Operation}' (attempt {Attempt} of {MaxRetries})", operationName, attempt, maxRetries);
                lastResult = new SmsResult(false, Error: ex.Message, Provider: _provider.ProviderType);
            }
        }

        _logger.LogError("All {MaxRetries} retries exhausted for '{Operation}'. Last error: {Error}",
            maxRetries, operationName, lastResult?.Error);

        return lastResult ?? new SmsResult(false, Error: "Unknown error after retries", Provider: _provider.ProviderType);
    }

    /// <summary>
    /// Determines whether an error message indicates a non-retryable condition.
    /// </summary>
    private static bool IsNonRetryableError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return false;

        // Common non-retryable patterns across SMS providers
        return error.Contains("invalid", StringComparison.OrdinalIgnoreCase)
            || error.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
            || error.Contains("forbidden", StringComparison.OrdinalIgnoreCase)
            || error.Contains("(400)", StringComparison.OrdinalIgnoreCase)
            || error.Contains("(401)", StringComparison.OrdinalIgnoreCase)
            || error.Contains("(403)", StringComparison.OrdinalIgnoreCase)
            || error.Contains("(404)", StringComparison.OrdinalIgnoreCase);
    }
}
