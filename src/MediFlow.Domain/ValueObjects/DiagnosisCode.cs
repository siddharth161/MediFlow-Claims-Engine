using MediFlow.Domain.Common;

namespace MediFlow.Domain.ValueObjects;

/// <summary>
/// Represents a CPT/HCPCS procedure code used in healthcare billing.
/// </summary>
public sealed class DiagnosisCode : ValueObject
{
    public string Code { get; }
    public string Description { get; }

    private DiagnosisCode(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static DiagnosisCode Create(string code, string description = "")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Diagnosis code cannot be empty.", nameof(code));

        return new DiagnosisCode(code.Trim().ToUpperInvariant(), description.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => $"{Code} - {Description}";
}
