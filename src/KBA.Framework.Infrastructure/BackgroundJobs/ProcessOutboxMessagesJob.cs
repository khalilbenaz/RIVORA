using System.Text.Json;
using KBA.Framework.Application.Mediator;
using KBA.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace KBA.Framework.Infrastructure.BackgroundJobs;

/// <summary>
/// Job Quartz pour le traitement des messages Outbox (Pattern Outbox 2.1)
/// </summary>
[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly KBADbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        KBADbContext dbContext,
        IMediator mediator,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Début du traitement des messages Outbox");

        var messages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        if (!messages.Any())
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("Type d'événement inconnu : {Type}", message.Type);
                    message.Error = $"Type d'événement inconnu : {message.Type}";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType);
                if (domainEvent == null)
                {
                    _logger.LogWarning("Impossible de désérialiser l'événement : {Type}", message.Type);
                    message.Error = "Erreur de désérialisation";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                // Publier l'événement via MediatR (ou le médiateur interne)
                // Note: Ici nous utilisons le IMediator du framework KBA
                await _mediator.Publish(domainEvent, context.CancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                _logger.LogInformation("Message Outbox traité avec succès : {Type}", message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du message Outbox {Id}", message.Id);
                message.Error = ex.Message;
                // On ne marque pas comme traité pour permettre un retry, 
                // mais on pourrait implémenter un compteur de tentatives.
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
