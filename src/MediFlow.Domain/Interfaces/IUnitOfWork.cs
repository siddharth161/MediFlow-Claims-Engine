namespace MediFlow.Domain.Interfaces;

/// <summary>
/// Unit of Work contract to coordinate transactional persistence across repositories.
/// </summary>
public interface IUnitOfWork
{
    IClaimRepository Claims { get; }
    IProviderRepository Providers { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
