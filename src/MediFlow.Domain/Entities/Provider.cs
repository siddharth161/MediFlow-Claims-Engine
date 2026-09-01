using MediFlow.Domain.Common;
using MediFlow.Domain.Enums;
using MediFlow.Domain.ValueObjects;

namespace MediFlow.Domain.Entities;

/// <summary>
/// Represents a healthcare provider (physician, facility, etc.) in the network.
/// Acts as an Aggregate Root for provider-related operations.
/// </summary>
public sealed class Provider : BaseEntity
{
    public NationalProviderId Npi { get; private set; } = default!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public string TaxonomyCode { get; private set; } = string.Empty;
    public ProviderNetworkStatus NetworkStatus { get; private set; }
    public string State { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // Navigation
    public ICollection<Claim> Claims { get; private set; } = [];

    private Provider() { } // EF Core

    public static Provider Create(
        string npi, string firstName, string lastName,
        string specialty, string taxonomyCode, string state,
        ProviderNetworkStatus networkStatus = ProviderNetworkStatus.InNetwork)
    {
        return new Provider
        {
            Npi = NationalProviderId.Create(npi),
            FirstName = firstName,
            LastName = lastName,
            Specialty = specialty,
            TaxonomyCode = taxonomyCode,
            State = state,
            NetworkStatus = networkStatus,
            IsActive = true
        };
    }

    public void Terminate()
    {
        NetworkStatus = ProviderNetworkStatus.Terminated;
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";
}
