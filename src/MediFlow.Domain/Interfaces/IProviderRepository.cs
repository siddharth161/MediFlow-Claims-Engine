using MediFlow.Domain.Entities;

namespace MediFlow.Domain.Interfaces;

/// <summary>
/// Repository contract for Provider aggregate persistence.
/// </summary>
public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Provider?> GetByNpiAsync(string npi, CancellationToken ct = default);
    Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Provider provider, CancellationToken ct = default);
    void Update(Provider provider);
}
