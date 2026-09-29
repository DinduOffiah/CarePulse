using CarePulse.Application.Features.Notes.DTOs;
using FluentValidation;

namespace CarePulse.Application.Features.Notes.Validators;

public class CreateConsultationNoteRequestValidator : AbstractValidator<CreateConsultationNoteRequest>
{
    public CreateConsultationNoteRequestValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Subjective).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Assessment).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Plan).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Prescriptions).MaximumLength(2000).When(x => x.Prescriptions is not null);
    }
}

public class UpdateConsultationNoteRequestValidator : AbstractValidator<UpdateConsultationNoteRequest>
{
    public UpdateConsultationNoteRequestValidator()
    {
        RuleFor(x => x.Subjective).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Assessment).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Plan).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Prescriptions).MaximumLength(2000).When(x => x.Prescriptions is not null);
    }
}
