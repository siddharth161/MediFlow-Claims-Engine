using MediatR;
using MediFlow.Application.Claims.Queries;
using MediFlow.Application.DTOs;
using MediFlow.Application.Mapping;
using MediFlow.Domain.Enums;
using MediFlow.Domain.Interfaces;

namespace MediFlow.Application.Claims.Handlers;

/// <summary>
/// Query handlers for retrieving claims and computing summary dashboard metrics.
/// </summary>
public sealed class GetClaimByIdHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetClaimByIdQuery, ClaimDto?>
{
    public async Task<ClaimDto?> Handle(GetClaimByIdQuery request, CancellationToken ct)
    {
        var claim = await unitOfWork.Claims.GetByIdAsync(request.ClaimId, ct);
        return claim?.ToDto();
    }
}

public sealed class GetAllClaimsHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetAllClaimsQuery, IReadOnlyList<ClaimDto>>
{
    public async Task<IReadOnlyList<ClaimDto>> Handle(GetAllClaimsQuery request, CancellationToken ct)
    {
        var claims = await unitOfWork.Claims.GetAllAsync(ct);
        return claims.Select(c => c.ToDto()).ToList();
    }
}

public sealed class GetClaimSummaryHandler(IUnitOfWork unitOfWork) 
    : IRequestHandler<GetClaimSummaryQuery, ClaimSummaryDto>
{
    public async Task<ClaimSummaryDto> Handle(GetClaimSummaryQuery request, CancellationToken ct)
    {
        var claims = await unitOfWork.Claims.GetAllAsync(ct);
        
        var total = claims.Count;
        var approved = claims.Count(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid);
        var denied = claims.Count(c => c.Status == ClaimStatus.Denied);
        var pending = claims.Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview);
        var totalBilled = claims.Sum(c => c.TotalBilledAmount?.Amount ?? 0m);
        var totalApproved = claims.Sum(c => c.ApprovedAmount?.Amount ?? 0m);

        return new ClaimSummaryDto(total, approved, denied, pending, totalBilled, totalApproved);
    }
}
