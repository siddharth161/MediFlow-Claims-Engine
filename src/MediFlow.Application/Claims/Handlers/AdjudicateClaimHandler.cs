using MediatR;
using MediFlow.Application.Claims.Commands;
using MediFlow.Application.DTOs;
using MediFlow.Application.Mapping;
using MediFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediFlow.Application.Claims.Handlers;

/// <summary>
/// Handles the AdjudicateClaimCommand — applies approval or denial business rules
/// on the Claim aggregate and persists the outcome.
/// </summary>
public sealed class AdjudicateClaimHandler(
    IUnitOfWork unitOfWork,
    ILogger<AdjudicateClaimHandler> logger) : IRequestHandler<AdjudicateClaimCommand, ClaimDto>
{
    public async Task<ClaimDto> Handle(AdjudicateClaimCommand request, CancellationToken ct)
    {
        var claim = await unitOfWork.Claims.GetByIdAsync(request.ClaimId, ct)
            ?? throw new KeyNotFoundException($"Claim '{request.ClaimId}' not found.");

        if (request.Approve)
        {
            var amount = request.ApprovedAmount
                ?? throw new ArgumentException("Approved amount is required when approving a claim.");
            claim.Approve(amount);
            logger.LogInformation("Claim {ClaimNumber} APPROVED for {Amount}", claim.ClaimNumber, amount);
        }
        else
        {
            var reason = request.DenialReason
                ?? throw new ArgumentException("Denial reason is required when denying a claim.");
            claim.Deny(reason);
            logger.LogInformation("Claim {ClaimNumber} DENIED: {Reason}", claim.ClaimNumber, reason);
        }

        unitOfWork.Claims.Update(claim);
        await unitOfWork.SaveChangesAsync(ct);

        return claim.ToDto();
    }
}
