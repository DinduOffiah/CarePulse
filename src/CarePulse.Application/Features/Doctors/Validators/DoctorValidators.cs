using CarePulse.Application.Features.Doctors.DTOs;
using FluentValidation;

namespace CarePulse.Application.Features.Doctors.Validators;

public class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LicenseNumber).MaximumLength(50).When(x => x.LicenseNumber is not null);
        RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio is not null);
        RuleFor(x => x.ConsultationDurationMinutes).InclusiveBetween(5, 240);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LicenseNumber).MaximumLength(50).When(x => x.LicenseNumber is not null);
        RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio is not null);
        RuleFor(x => x.ConsultationDurationMinutes).InclusiveBetween(5, 240);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
    }
}

public class SetAvailabilityRequestValidator : AbstractValidator<SetAvailabilityRequest>
{
    public SetAvailabilityRequestValidator()
    {
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime)
            .WithMessage("Start time must be before end time.");
    }
}
