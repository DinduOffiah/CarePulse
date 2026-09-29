using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Commands;

public record UpdateAppointmentStatusCommand(Guid Id, UpdateAppointmentStatusRequest Request)
    : IRequest<ApiResponse<AppointmentDto>>;

public class UpdateAppointmentStatusCommandHandler
    : IRequestHandler<UpdateAppointmentStatusCommand, ApiResponse<AppointmentDto>>
{
    private readonly IApplicationDbContext _db;

    // Allowed transitions (simple status machine)
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> AllowedTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.Confirmed] = [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.CheckedIn] = [AppointmentStatus.InProgress, AppointmentStatus.NoShow],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    public UpdateAppointmentStatusCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<AppointmentDto>> Handle(UpdateAppointmentStatusCommand command, CancellationToken ct)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == command.Id, ct);

        if (appointment is null)
            return ApiResponse<AppointmentDto>.Fail("Appointment not found.");

        var newStatus = command.Request.Status;

        if (!AllowedTransitions.TryGetValue(appointment.Status, out var allowed) || !allowed.Contains(newStatus))
        {
            return ApiResponse<AppointmentDto>.Fail(
                $"Invalid status transition from '{appointment.Status}' to '{newStatus}'.");
        }

        appointment.Status = newStatus;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var dto = new AppointmentDto(
            appointment.Id, appointment.PatientId, appointment.Patient.User.FullName,
            appointment.DoctorId, appointment.Doctor.User.FullName, appointment.Doctor.Specialty,
            appointment.StartTime, appointment.EndTime, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason, appointment.CreatedAt);

        return ApiResponse<AppointmentDto>.Ok(dto, $"Appointment status updated to '{newStatus}'.");
    }
}
