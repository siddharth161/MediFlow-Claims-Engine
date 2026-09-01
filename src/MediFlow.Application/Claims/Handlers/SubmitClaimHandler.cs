using MediatR;
using MediFlow.Application.Claims.Commands;
using MediFlow.Application.DTOs;
using MediFlow.Application.Mapping;
using MediFlow.Domain.Entities;
using MediFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediFlow.Application.Claims.Handlers;

/// <summary>
/// Handles the SubmitClaimCommand — validates the provider exists and is active,
/// creates the Claim aggregate, attaches line items, and persists the transaction.
/// </summary>
public sealed class SubmitClaimHandler(
    IUnitOfWork unitOfWork,
    ILogger<SubmitClaimHandler> logger) : IRequestHandler<SubmitClaimCommand, ClaimDto>
{
    public async Task<ClaimDto> Handle(SubmitClaimCommand request, CancellationToken ct)
    {
        // Validate provider exists and is in-network
        var provider = await unitOfWork.Providers.GetByIdAsync(request.ProviderId, ct)
            ?? throw new KeyNotFoundException($"Provider '{request.ProviderId}' not found.");

        if (!provider.IsActive)
            throw new InvalidOperationException($"Provider '{provider.FullName}' is not active.");

        // Create claim aggregate
        var claim = Claim.Submit(
            request.PatientId,
            request.PatientName,
            provider.Id,
            request.ClaimType,
            request.DiagnosisCode,
            request.DiagnosisDescription,
            request.DateOfService);

        // Add line items
        foreach (var li in request.LineItems)
        {
            claim.AddLineItem(li.ProcedureCode, li.Description, li.ServiceType, li.BilledAmount, li.Units);
        }

        await unitOfWork.Claims.AddAsync(claim, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Claim {ClaimNumber} submitted for patient {PatientId}, total billed: {Amount}",
            claim.ClaimNumber, claim.PatientId, claim.TotalBilledAmount);

        // Re-fetch with navigation properties for the DTO
        var saved = await unitOfWork.Claims.GetByIdAsync(claim.Id, ct);
        return saved!.ToDto();
    }
}
