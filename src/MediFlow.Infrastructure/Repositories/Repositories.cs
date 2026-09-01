using MediFlow.Domain.Entities;
using MediFlow.Domain.Interfaces;
using MediFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediFlow.Infrastructure.Repositories;

public sealed class ClaimRepository(MediFlowDbContext context) : IClaimRepository
{
    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Claims
            .Include(c => c.Provider)
            .Include(c => c.LineItems)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken ct = default)
    {
        return await context.Claims
            .Include(c => c.Provider)
            .Include(c => c.LineItems)
            .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber, ct);
    }

    public async Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Claims
            .Include(c => c.Provider)
            .Include(c => c.LineItems)
            .OrderByDescending(c => c.SubmittedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Claim>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default)
    {
        return await context.Claims
            .Include(c => c.LineItems)
            .Where(c => c.ProviderId == providerId)
            .OrderByDescending(c => c.SubmittedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Claim claim, CancellationToken ct = default)
    {
        await context.Claims.AddAsync(claim, ct);
    }

    public void Update(Claim claim)
    {
        context.Claims.Update(claim);
    }
}

public sealed class ProviderRepository(MediFlowDbContext context) : IProviderRepository
{
    public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Providers
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Provider?> GetByNpiAsync(string npi, CancellationToken ct = default)
    {
        return await context.Providers
            .FirstOrDefaultAsync(p => p.Npi == Domain.ValueObjects.NationalProviderId.Create(npi), ct);
    }

    public async Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Providers
            .OrderBy(p => p.LastName)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Provider provider, CancellationToken ct = default)
    {
        await context.Providers.AddAsync(provider, ct);
    }

    public void Update(Provider provider)
    {
        context.Providers.Update(provider);
    }
}

public sealed class UnitOfWork(
    MediFlowDbContext context,
    IClaimRepository claims,
    IProviderRepository providers) : IUnitOfWork
{
    public IClaimRepository Claims => claims;
    public IProviderRepository Providers => providers;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
