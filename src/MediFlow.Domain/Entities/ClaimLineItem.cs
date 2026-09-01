using MediFlow.Domain.Common;
using MediFlow.Domain.Enums;
using MediFlow.Domain.ValueObjects;

namespace MediFlow.Domain.Entities;

/// <summary>
/// Represents a single service line within a healthcare claim (e.g., a procedure or lab test).
/// </summary>
public sealed class ClaimLineItem : BaseEntity
{
    public Guid ClaimId { get; private set; }
    public string ProcedureCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ServiceType ServiceType { get; private set; }
    public Money BilledAmount { get; private set; } = default!;
    public int Units { get; private set; }

    // Navigation
    public Claim Claim { get; private set; } = default!;

    private ClaimLineItem() { } // EF Core

    public static ClaimLineItem Create(
        Guid claimId, string procedureCode, string description,
        ServiceType serviceType, decimal billedAmount, int units = 1)
    {
        if (units <= 0)
            throw new ArgumentException("Units must be greater than zero.", nameof(units));

        return new ClaimLineItem
        {
            ClaimId = claimId,
            ProcedureCode = procedureCode,
            Description = description,
            ServiceType = serviceType,
            BilledAmount = Money.Create(billedAmount * units),
            Units = units
        };
    }
}
