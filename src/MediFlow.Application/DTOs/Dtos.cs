using MediFlow.Domain.Enums;

namespace MediFlow.Application.DTOs;

public sealed record ClaimDto(
    Guid Id,
    string ClaimNumber,
    string PatientId,
    string PatientName,
    Guid ProviderId,
    string ProviderName,
    string ClaimType,
    string Status,
    string PrimaryDiagnosisCode,
    string PrimaryDiagnosisDescription,
    decimal TotalBilledAmount,
    decimal ApprovedAmount,
    DateTime DateOfService,
    DateTime SubmittedAtUtc,
    string? DenialReason,
    List<ClaimLineItemDto> LineItems);

public sealed record ClaimLineItemDto(
    Guid Id,
    string ProcedureCode,
    string Description,
    string ServiceType,
    decimal BilledAmount,
    int Units);

public sealed record ProviderDto(
    Guid Id,
    string Npi,
    string FirstName,
    string LastName,
    string Specialty,
    string TaxonomyCode,
    string State,
    string NetworkStatus,
    bool IsActive);

public sealed record ClaimSummaryDto(
    int TotalClaims,
    int Approved,
    int Denied,
    int Pending,
    decimal TotalBilledAmount,
    decimal TotalApprovedAmount);
