using MediFlow.Domain.Common;
using MediFlow.Domain.Enums;

namespace MediFlow.Domain.Events;

/// <summary>
/// Raised when a new healthcare claim is submitted into the system.
/// </summary>
public sealed class ClaimSubmittedEvent(Guid claimId, string claimNumber, string patientId) : DomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public string ClaimNumber { get; } = claimNumber;
    public string PatientId { get; } = patientId;
}

/// <summary>
/// Raised when a claim has been adjudicated (approved or denied).
/// </summary>
public sealed class ClaimAdjudicatedEvent(
    Guid claimId, string claimNumber,
    ClaimStatus outcome, decimal approvedAmount) : DomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public string ClaimNumber { get; } = claimNumber;
    public ClaimStatus Outcome { get; } = outcome;
    public decimal ApprovedAmount { get; } = approvedAmount;
}
