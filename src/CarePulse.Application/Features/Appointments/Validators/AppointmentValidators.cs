using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Enums;
using FluentValidation;

namespace CarePulse.Application.Features.Appointments.Validators;

public class BookAppointmentRequestValidator : AbstractValidator<BookAppointmentRequest>
{
    public BookAppointmentRequestValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.StartTime)
            .NotEmpty()
            .Must(t => t > DateTime.UtcNow)
            .WithMessage("Appointment start time must be in the future.");
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
    }
}

public class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(x => x.NewStartTime)
            .NotEmpty()
            .Must(t => t > DateTime.UtcNow)
            .WithMessage("New start time must be in the future.");
    }
}

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.CancellationReason).MaximumLength(500).When(x => x.CancellationReason is not null);
    }
}

public class UpdateAppointmentStatusRequestValidator : AbstractValidator<UpdateAppointmentStatusRequest>
{
    public UpdateAppointmentStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
