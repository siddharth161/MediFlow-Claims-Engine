using FluentAssertions;
using MediFlow.Application.Claims.Commands;
using MediFlow.Application.Validators;
using MediFlow.Domain.Enums;
using Xunit;

namespace MediFlow.UnitTests.Validators;

public class ValidatorTests
{
    private readonly SubmitClaimCommandValidator _submitValidator = new();
    private readonly AdjudicateClaimCommandValidator _adjudicateValidator = new();

    [Fact]
    public void SubmitClaimCommandValidator_WhenValid_ShouldNotHaveValidationError()
    {
        var command = new SubmitClaimCommand(
            "PAT-101", "Alice Johnson", Guid.NewGuid(), ClaimType.Professional,
            "I10", "Hypertension", DateTime.UtcNow.AddDays(-1),
            [new AddLineItemRequest("99214", "Office Visit", ServiceType.Consultation, 150m, 1)]);

        var result = _submitValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SubmitClaimCommandValidator_WhenLineItemsEmpty_ShouldFail()
    {
        var command = new SubmitClaimCommand(
            "PAT-101", "Alice Johnson", Guid.NewGuid(), ClaimType.Professional,
            "I10", "Hypertension", DateTime.UtcNow.AddDays(-1), []);

        var result = _submitValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LineItems");
    }

    [Fact]
    public void AdjudicateClaimCommandValidator_WhenRejectingWithoutReason_ShouldFail()
    {
        var command = new AdjudicateClaimCommand(Guid.NewGuid(), Approve: false, ApprovedAmount: null, DenialReason: "");

        var result = _adjudicateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DenialReason");
    }
}
