using System.Text.Json;
using RVR.Framework.Application.Mediator;
using RVR.Framework.Domain.Events;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace RVR.Framework.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz job for processing Outbox messages (Outbox Pattern 2.1).
/// Supports retry with configurable max retries and dead-letter status.
/// </summary>
[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly RVRDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        RVRDbContext dbContext,
        IMediator mediator,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Starting Outbox message processing");

        var messages = await _dbContext.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending || m.Status == OutboxMessageStatus.Failed)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        if (!messages.Any())
        {
            return;
        }

        foreach (var message in messages)
        {
            // Check if max retries exceeded - move to dead letter
            if (message.RetryCount >= message.MaxRetries)
            {
                message.Status = OutboxMessageStatus.DeadLetter;
                message.ProcessedOnUtc = DateTime.UtcNow;
                _logger.LogWarning(
                    "Outbox message {Id} (type: {Type}) moved to dead letter after {RetryCount} retries. Last error: {LastError}",
                    message.Id, message.Type, message.RetryCount, message.LastError);
                continue;
            }

            message.Status = OutboxMessageStatus.Processing;
            message.RetryCount++;

            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Unknown event type: {Type}", message.Type);
                    message.Error = $"Unknown event type: {message.Type}";
                    message.LastError = message.Error;
                    message.Status = OutboxMessageStatus.DeadLetter;
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType);
                if (domainEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize event: {Type}", message.Type);
                    message.Error = "Deserialization error";
                    message.LastError = message.Error;
                    message.Status = OutboxMessageStatus.DeadLetter;
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                // Publish the event via the framework mediator
                await _mediator.Publish(domainEvent, context.CancellationToken);

                message.Status = OutboxMessageStatus.Processed;
                message.ProcessedOnUtc = DateTime.UtcNow;
                _logger.LogInformation("Outbox message processed successfully: {Type}", message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Outbox message {Id} (attempt {RetryCount}/{MaxRetries})",
                    message.Id, message.RetryCount, message.MaxRetries);

                message.Error = ex.Message;
                message.LastError = ex.Message;

                if (message.RetryCount >= message.MaxRetries)
                {
                    message.Status = OutboxMessageStatus.DeadLetter;
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    _logger.LogWarning(
                        "Outbox message {Id} moved to dead letter after {RetryCount} retries",
                        message.Id, message.RetryCount);
                }
                else
                {
                    message.Status = OutboxMessageStatus.Failed;
                }
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
