using MediFlow.Application.DTOs;
using MediFlow.Domain.Entities;

namespace MediFlow.Application.Mapping;

/// <summary>
/// Manual mapping extensions from domain entities to DTOs.
/// Avoids AutoMapper dependency for transparency and performance.
/// </summary>
public static class ClaimMapper
{
    public static ClaimDto ToDto(this Claim claim)
    {
        return new ClaimDto(
            claim.Id,
            claim.ClaimNumber,
            claim.PatientId,
            claim.PatientName,
            claim.ProviderId,
            claim.Provider?.FullName ?? "Unknown",
            claim.ClaimType.ToString(),
            claim.Status.ToString(),
            claim.PrimaryDiagnosis.Code,
            claim.PrimaryDiagnosis.Description,
            claim.TotalBilledAmount.Amount,
            claim.ApprovedAmount.Amount,
            claim.DateOfService,
            claim.SubmittedAtUtc,
            claim.DenialReason,
            claim.LineItems.Select(li => li.ToDto()).ToList());
    }

    public static ClaimLineItemDto ToDto(this ClaimLineItem lineItem)
    {
        return new ClaimLineItemDto(
            lineItem.Id,
            lineItem.ProcedureCode,
            lineItem.Description,
            lineItem.ServiceType.ToString(),
            lineItem.BilledAmount.Amount,
            lineItem.Units);
    }

    public static ProviderDto ToDto(this Provider provider)
    {
        return new ProviderDto(
            provider.Id,
            provider.Npi.Value,
            provider.FirstName,
            provider.LastName,
            provider.Specialty,
            provider.TaxonomyCode,
            provider.State,
            provider.NetworkStatus.ToString(),
            provider.IsActive);
    }
}
