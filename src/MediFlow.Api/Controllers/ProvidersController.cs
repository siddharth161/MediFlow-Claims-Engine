using MediFlow.Application.DTOs;
using MediFlow.Application.Mapping;
using MediFlow.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ProvidersController(IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>
    /// Retrieves all registered healthcare providers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProviderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProviderDto>>> GetAll(CancellationToken ct)
    {
        var providers = await unitOfWork.Providers.GetAllAsync(ct);
        return Ok(providers.Select(p => p.ToDto()).ToList());
    }

    /// <summary>
    /// Retrieves a provider by their unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderDto>> GetById(Guid id, CancellationToken ct)
    {
        var provider = await unitOfWork.Providers.GetByIdAsync(id, ct);
        return provider is null ? NotFound() : Ok(provider.ToDto());
    }
}
