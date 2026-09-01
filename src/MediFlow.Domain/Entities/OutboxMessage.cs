using MediFlow.Domain.Common;

namespace MediFlow.Domain.Entities;

/// <summary>
/// Transactional Outbox entry for reliable domain event publishing.
/// Events are persisted atomically with the business transaction,
/// then dispatched asynchronously by a background processor.
/// </summary>
public sealed class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
