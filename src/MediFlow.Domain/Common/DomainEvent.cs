using MediatR;

namespace MediFlow.Domain.Common;

/// <summary>
/// Marker base class for all domain events dispatched via MediatR.
/// </summary>
public abstract class DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
