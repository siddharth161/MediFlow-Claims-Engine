using FluentValidation;
using MediFlow.Application.Claims.Commands;

namespace MediFlow.Application.Validators;

public sealed class SubmitClaimCommandValidator : AbstractValidator<SubmitClaimCommand>
{
    public SubmitClaimCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.")
            .MaximumLength(50).WithMessage("Patient ID must not exceed 50 characters.");

        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient Name is required.")
            .MaximumLength(100).WithMessage("Patient Name must not exceed 100 characters.");

        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider ID is required.");

        RuleFor(x => x.DiagnosisCode)
            .NotEmpty().WithMessage("Diagnosis code is required.")
            .Matches(@"^[A-Z0-9\.]+$").WithMessage("Diagnosis code must follow standard ICD-10 format.");

        RuleFor(x => x.DateOfService)
            .NotEmpty().WithMessage("Date of service is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Date of service cannot be in the future.");

        RuleFor(x => x.LineItems)
            .NotEmpty().WithMessage("Claim must contain at least one line item.");

        RuleForEach(x => x.LineItems).ChildRules(line =>
        {
            line.RuleFor(l => l.ProcedureCode)
                .NotEmpty().WithMessage("Procedure code (CPT/HCPCS) is required.");
            line.RuleFor(l => l.BilledAmount)
                .GreaterThan(0).WithMessage("Billed amount must be greater than zero.");
            line.RuleFor(l => l.Units)
                .GreaterThan(0).WithMessage("Units must be at least 1.");
        });
    }
}

public sealed class AdjudicateClaimCommandValidator : AbstractValidator<AdjudicateClaimCommand>
{
    public AdjudicateClaimCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required.");

        When(x => x.Approve, () =>
        {
            RuleFor(x => x.ApprovedAmount)
                .NotNull().WithMessage("Approved amount is required when approving a claim.")
                .GreaterThanOrEqualTo(0).WithMessage("Approved amount cannot be negative.");
        });

        When(x => !x.Approve, () =>
        {
            RuleFor(x => x.DenialReason)
                .NotEmpty().WithMessage("Denial reason is mandatory when rejecting a claim.")
                .MaximumLength(500).WithMessage("Denial reason cannot exceed 500 characters.");
        });
    }
}
