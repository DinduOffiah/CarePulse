using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Commands;

public record CancelAppointmentCommand(Guid Id, CancelAppointmentRequest Request)
    : IRequest<ApiResponse<AppointmentDto>>;

public class CancelAppointmentCommandHandler
    : IRequestHandler<CancelAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly IApplicationDbContext _db;

    public CancelAppointmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<AppointmentDto>> Handle(CancelAppointmentCommand command, CancellationToken ct)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == command.Id, ct);

        if (appointment is null)
            return ApiResponse<AppointmentDto>.Fail("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return ApiResponse<AppointmentDto>.Fail($"Cannot cancel an appointment with status '{appointment.Status}'.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = command.Request.CancellationReason;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var dto = new AppointmentDto(
            appointment.Id, appointment.PatientId, appointment.Patient.User.FullName,
            appointment.DoctorId, appointment.Doctor.User.FullName, appointment.Doctor.Specialty,
            appointment.StartTime, appointment.EndTime, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason, appointment.CreatedAt);

        return ApiResponse<AppointmentDto>.Ok(dto, "Appointment cancelled successfully.");
    }
}
