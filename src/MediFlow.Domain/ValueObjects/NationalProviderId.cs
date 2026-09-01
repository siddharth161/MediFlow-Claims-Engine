using MediFlow.Domain.Common;

namespace MediFlow.Domain.ValueObjects;

/// <summary>
/// Represents a National Provider Identifier (NPI) — a 10-digit unique identifier
/// assigned to healthcare providers in the United States.
/// </summary>
public sealed class NationalProviderId : ValueObject
{
    public string Value { get; }

    private NationalProviderId(string value) => Value = value;

    public static NationalProviderId Create(string npi)
    {
        if (string.IsNullOrWhiteSpace(npi))
            throw new ArgumentException("NPI cannot be empty.", nameof(npi));

        var trimmed = npi.Trim();
        if (trimmed.Length != 10 || !trimmed.All(char.IsDigit))
            throw new ArgumentException("NPI must be exactly 10 digits.", nameof(npi));

        return new NationalProviderId(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
