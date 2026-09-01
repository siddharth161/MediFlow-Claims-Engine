using MediatR;
using MediFlow.Application.DTOs;

namespace MediFlow.Application.Claims.Queries;

/// <summary>
/// Query to retrieve a single claim by its unique identifier.
/// </summary>
public sealed record GetClaimByIdQuery(Guid ClaimId) : IRequest<ClaimDto?>;

/// <summary>
/// Query to retrieve all claims in the system.
/// </summary>
public sealed record GetAllClaimsQuery() : IRequest<IReadOnlyList<ClaimDto>>;

/// <summary>
/// Query to get a summary dashboard of claim statistics.
/// </summary>
public sealed record GetClaimSummaryQuery() : IRequest<ClaimSummaryDto>;
