using MediatR;
using MediFlow.Application.DTOs;
using MediFlow.Domain.Enums;

namespace MediFlow.Application.Claims.Commands;

/// <summary>
/// Command to submit a new healthcare claim into the system.
/// </summary>
public sealed record SubmitClaimCommand(
    string PatientId,
    string PatientName,
    Guid ProviderId,
    ClaimType ClaimType,
    string DiagnosisCode,
    string DiagnosisDescription,
    DateTime DateOfService,
    List<AddLineItemRequest> LineItems) : IRequest<ClaimDto>;

public sealed record AddLineItemRequest(
    string ProcedureCode,
    string Description,
    ServiceType ServiceType,
    decimal BilledAmount,
    int Units);
