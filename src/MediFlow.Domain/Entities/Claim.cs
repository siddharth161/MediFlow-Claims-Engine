using MediFlow.Domain.Common;
using MediFlow.Domain.Enums;
using MediFlow.Domain.Events;
using MediFlow.Domain.ValueObjects;

namespace MediFlow.Domain.Entities;

/// <summary>
/// Represents a healthcare insurance claim submitted by a provider for a patient.
/// This is the primary Aggregate Root of the Claims bounded context.
/// </summary>
public sealed class Claim : BaseEntity
{
    public string ClaimNumber { get; private set; } = string.Empty;
    public string PatientId { get; private set; } = string.Empty;
    public string PatientName { get; private set; } = string.Empty;
    public Guid ProviderId { get; private set; }
    public ClaimType ClaimType { get; private set; }
    public ClaimStatus Status { get; private set; }
    public DiagnosisCode PrimaryDiagnosis { get; private set; } = default!;
    public Money TotalBilledAmount { get; private set; } = default!;
    public Money ApprovedAmount { get; private set; } = default!;
    public DateTime DateOfService { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public string? DenialReason { get; private set; }

    // Navigation
    public Provider Provider { get; private set; } = default!;
    public ICollection<ClaimLineItem> LineItems { get; private set; } = [];

    private Claim() { } // EF Core

    public static Claim Submit(
        string patientId, string patientName,
        Guid providerId, ClaimType claimType,
        string diagnosisCode, string diagnosisDescription,
        DateTime dateOfService)
    {
        var claim = new Claim
        {
            ClaimNumber = GenerateClaimNumber(),
            PatientId = patientId,
            PatientName = patientName,
            ProviderId = providerId,
            ClaimType = claimType,
            PrimaryDiagnosis = DiagnosisCode.Create(diagnosisCode, diagnosisDescription),
            TotalBilledAmount = Money.Zero(),
            ApprovedAmount = Money.Zero(),
            DateOfService = dateOfService,
            Status = ClaimStatus.Submitted,
            SubmittedAtUtc = DateTime.UtcNow
        };

        claim.AddDomainEvent(new ClaimSubmittedEvent(claim.Id, claim.ClaimNumber, claim.PatientId));
        return claim;
    }

    public void AddLineItem(string procedureCode, string description, ServiceType serviceType, decimal billedAmount, int units = 1)
    {
        var lineItem = ClaimLineItem.Create(Id, procedureCode, description, serviceType, billedAmount, units);
        LineItems.Add(lineItem);
        RecalculateTotalBilledAmount();
    }

    public void Approve(decimal approvedAmount)
    {
        if (Status != ClaimStatus.Submitted && Status != ClaimStatus.UnderReview)
            throw new InvalidOperationException($"Cannot approve claim in status '{Status}'.");

        Status = ClaimStatus.Approved;
        ApprovedAmount = Money.Create(approvedAmount);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ClaimAdjudicatedEvent(Id, ClaimNumber, ClaimStatus.Approved, approvedAmount));
    }

    public void Deny(string reason)
    {
        if (Status != ClaimStatus.Submitted && Status != ClaimStatus.UnderReview)
            throw new InvalidOperationException($"Cannot deny claim in status '{Status}'.");

        Status = ClaimStatus.Denied;
        DenialReason = reason;
        ApprovedAmount = Money.Zero();
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ClaimAdjudicatedEvent(Id, ClaimNumber, ClaimStatus.Denied, 0));
    }

    public void MarkUnderReview()
    {
        if (Status != ClaimStatus.Submitted)
            throw new InvalidOperationException("Only submitted claims can be placed under review.");

        Status = ClaimStatus.UnderReview;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void RecalculateTotalBilledAmount()
    {
        var total = LineItems.Sum(li => li.BilledAmount.Amount);
        TotalBilledAmount = Money.Create(total);
    }

    private static string GenerateClaimNumber()
    {
        return $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
