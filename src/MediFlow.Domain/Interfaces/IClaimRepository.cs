using MediFlow.Domain.Entities;

namespace MediFlow.Domain.Interfaces;

/// <summary>
/// Repository contract for Claim aggregate persistence.
/// </summary>
public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Claim>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default);
    Task AddAsync(Claim claim, CancellationToken ct = default);
    void Update(Claim claim);
}
