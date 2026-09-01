using MediatR;
using MediFlow.Application.Claims.Commands;
using MediFlow.Application.Claims.Queries;
using MediFlow.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MediFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ClaimsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Submits a new healthcare claim into the engine.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimDto>> SubmitClaim([FromBody] SubmitClaimCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetClaimById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a claim by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> GetClaimById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetClaimByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Retrieves all healthcare claims.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClaimDto>>> GetAllClaims(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllClaimsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Adjudicates (approves or denies) a submitted claim.
    /// </summary>
    [HttpPost("{id:guid}/adjudicate")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> AdjudicateClaim(Guid id, [FromBody] AdjudicateClaimRequest request, CancellationToken ct)
    {
        var command = new AdjudicateClaimCommand(id, request.Approve, request.ApprovedAmount, request.DenialReason);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregated metrics summary of all claims for dashboard visualization.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ClaimSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClaimSummaryDto>> GetSummary(CancellationToken ct)
    {
        var result = await sender.Send(new GetClaimSummaryQuery(), ct);
        return Ok(result);
    }
}

public sealed record AdjudicateClaimRequest(
    bool Approve,
    decimal? ApprovedAmount,
    string? DenialReason);
