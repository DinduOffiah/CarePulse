using CarePulse.Application.Features.Patients.DTOs;
using FluentValidation;

namespace CarePulse.Application.Features.Patients.Validators;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Gender).MaximumLength(20).When(x => x.Gender is not null);
        RuleFor(x => x.BloodGroup).MaximumLength(10).When(x => x.BloodGroup is not null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null);
        RuleFor(x => x.EmergencyContactName).MaximumLength(150).When(x => x.EmergencyContactName is not null);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(30).When(x => x.EmergencyContactPhone is not null);
    }
}

public class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequest>
{
    public UpdatePatientRequestValidator()
    {
        RuleFor(x => x.Gender).MaximumLength(20).When(x => x.Gender is not null);
        RuleFor(x => x.BloodGroup).MaximumLength(10).When(x => x.BloodGroup is not null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null);
        RuleFor(x => x.EmergencyContactName).MaximumLength(150).When(x => x.EmergencyContactName is not null);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(30).When(x => x.EmergencyContactPhone is not null);
    }
}
