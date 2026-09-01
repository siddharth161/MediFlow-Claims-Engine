using MediFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MediFlow.Infrastructure.Background;

/// <summary>
/// Background worker implementing the Transactional Outbox pattern.
/// Periodically queries un-dispatched events from the database and publishes them reliably.
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Transactional Outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MediFlowDbContext>();

                var pendingMessages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedAtUtc == null && m.RetryCount < 3)
                    .OrderBy(m => m.CreatedAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        // Simulate publishing to event bus (Kafka / RabbitMQ)
                        logger.LogInformation("[Outbox Event Published] Type: {EventType} | Payload: {Payload}",
                            message.EventType, message.Payload);

                        message.ProcessedAtUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        message.Error = ex.Message;
                        logger.LogError(ex, "Failed to publish Outbox message {Id}", message.Id);
                    }
                }

                if (pendingMessages.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing Outbox messages.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        logger.LogInformation("Transactional Outbox processor stopped.");
    }
}
