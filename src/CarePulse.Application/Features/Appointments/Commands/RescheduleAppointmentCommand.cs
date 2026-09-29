using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Commands;

public record RescheduleAppointmentCommand(Guid Id, RescheduleAppointmentRequest Request)
    : IRequest<ApiResponse<AppointmentDto>>;

public class RescheduleAppointmentCommandHandler
    : IRequestHandler<RescheduleAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly IApplicationDbContext _db;

    public RescheduleAppointmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<AppointmentDto>> Handle(RescheduleAppointmentCommand command, CancellationToken ct)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Availabilities)
            .FirstOrDefaultAsync(a => a.Id == command.Id, ct);

        if (appointment is null)
            return ApiResponse<AppointmentDto>.Fail("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            return ApiResponse<AppointmentDto>.Fail($"Cannot reschedule an appointment with status '{appointment.Status}'.");

        var newStart = DateTime.SpecifyKind(command.Request.NewStartTime, DateTimeKind.Utc);
        var duration = (appointment.EndTime - appointment.StartTime).TotalMinutes;
        var newEnd = newStart.AddMinutes(duration);

        // Availability check
        var dayOfWeek = newStart.DayOfWeek;
        var startTimeOnly = TimeOnly.FromDateTime(newStart);
        var endTimeOnly = TimeOnly.FromDateTime(newEnd);

        var isAvailable = appointment.Doctor.Availabilities
            .Where(a => a.DeletedAt == null && a.IsActive && a.DayOfWeek == dayOfWeek)
            .Any(a => a.StartTime <= startTimeOnly && a.EndTime >= endTimeOnly);

        if (!isAvailable)
            return ApiResponse<AppointmentDto>.Fail("Doctor is not available at the new requested time.");

        // Conflict check (exclude self)
        var hasConflict = await _db.Appointments
            .AnyAsync(a =>
                a.Id != appointment.Id &&
                a.DoctorId == appointment.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                a.StartTime < newEnd &&
                a.EndTime > newStart, ct);

        if (hasConflict)
            return ApiResponse<AppointmentDto>.Fail("The new time slot is already booked.");

        appointment.StartTime = newStart;
        appointment.EndTime = newEnd;
        appointment.Status = AppointmentStatus.Scheduled;
        appointment.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<AppointmentDto>.Fail("Concurrency conflict. Please reload and try again.");
        }

        var dto = new AppointmentDto(
            appointment.Id, appointment.PatientId, appointment.Patient.User.FullName,
            appointment.DoctorId, appointment.Doctor.User.FullName, appointment.Doctor.Specialty,
            appointment.StartTime, appointment.EndTime, appointment.Status,
            appointment.Reason, appointment.Notes, appointment.CancellationReason, appointment.CreatedAt);

        return ApiResponse<AppointmentDto>.Ok(dto, "Appointment rescheduled successfully.");
    }
}
