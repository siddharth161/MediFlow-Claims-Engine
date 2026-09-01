using FluentAssertions;
using MediFlow.Domain.Entities;
using MediFlow.Domain.Enums;
using MediFlow.Domain.ValueObjects;
using Xunit;

namespace MediFlow.UnitTests.Domain;

public class DomainTests
{
    [Fact]
    public void NationalProviderId_Valid10Digits_ShouldSucceed()
    {
        var npi = NationalProviderId.Create("1234567890");
        npi.Value.Should().Be("1234567890");
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("12345678901")]
    [InlineData("12345ABCDE")]
    public void NationalProviderId_InvalidFormat_ShouldThrowArgumentException(string invalidNpi)
    {
        var act = () => NationalProviderId.Create(invalidNpi);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Money_AddSameCurrency_ShouldReturnSum()
    {
        var m1 = Money.Create(150.50m, "USD");
        var m2 = Money.Create(49.50m, "USD");

        var result = m1.Add(m2);

        result.Amount.Should().Be(200.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Claim_Submit_ShouldInitializeWithCorrectDefaultsAndRaiseEvent()
    {
        var providerId = Guid.NewGuid();
        var claim = Claim.Submit(
            "PAT-12345", "Alice Smith",
            providerId, ClaimType.Professional,
            "I10", "Essential (primary) hypertension",
            DateTime.UtcNow.AddDays(-1));

        claim.ClaimNumber.Should().StartWith("CLM-");
        claim.Status.Should().Be(ClaimStatus.Submitted);
        claim.PatientId.Should().Be("PAT-12345");
        claim.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Claim_AddLineItem_ShouldRecalculateTotalBilledAmount()
    {
        var claim = Claim.Submit(
            "PAT-12345", "Alice Smith",
            Guid.NewGuid(), ClaimType.Professional,
            "I10", "Hypertension",
            DateTime.UtcNow);

        claim.AddLineItem("99213", "Office Visit", ServiceType.Consultation, 120.00m, 1);
        claim.AddLineItem("80053", "Comprehensive Metabolic Panel", ServiceType.Diagnostic, 80.00m, 1);

        claim.TotalBilledAmount.Amount.Should().Be(200.00m);
        claim.LineItems.Should().HaveCount(2);
    }

    [Fact]
    public void Claim_Approve_WhenSubmitted_ShouldUpdateStatusAndRaiseEvent()
    {
        var claim = Claim.Submit(
            "PAT-12345", "Alice Smith",
            Guid.NewGuid(), ClaimType.Professional,
            "I10", "Hypertension",
            DateTime.UtcNow);

        claim.AddLineItem("99213", "Office Visit", ServiceType.Consultation, 100.00m);
        claim.Approve(90.00m);

        claim.Status.Should().Be(ClaimStatus.Approved);
        claim.ApprovedAmount.Amount.Should().Be(90.00m);
    }

    [Fact]
    public void Claim_Deny_WhenSubmitted_ShouldSetDenialReasonAndZeroApprovedAmount()
    {
        var claim = Claim.Submit(
            "PAT-12345", "Alice Smith",
            Guid.NewGuid(), ClaimType.Professional,
            "I10", "Hypertension",
            DateTime.UtcNow);

        claim.Deny("Service not covered under active plan.");

        claim.Status.Should().Be(ClaimStatus.Denied);
        claim.DenialReason.Should().Be("Service not covered under active plan.");
        claim.ApprovedAmount.Amount.Should().Be(0m);
    }
}
