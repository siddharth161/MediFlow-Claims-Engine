using MediatR;
using MediFlow.Application.DTOs;

namespace MediFlow.Application.Claims.Commands;

/// <summary>
/// Command to adjudicate (approve or deny) an existing claim.
/// </summary>
public sealed record AdjudicateClaimCommand(
    Guid ClaimId,
    bool Approve,
    decimal? ApprovedAmount,
    string? DenialReason) : IRequest<ClaimDto>;
