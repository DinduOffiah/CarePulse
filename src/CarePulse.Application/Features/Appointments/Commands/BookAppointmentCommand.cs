using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Commands;

public record BookAppointmentCommand(BookAppointmentRequest Request) : IRequest<ApiResponse<AppointmentDto>>;

public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, ApiResponse<AppointmentDto>>
{
    private readonly IApplicationDbContext _db;

    public BookAppointmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<AppointmentDto>> Handle(BookAppointmentCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var patient = await _db.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == req.PatientId, ct);
        if (patient is null)
            return ApiResponse<AppointmentDto>.Fail("Patient not found.");

        var doctor = await _db.Doctors
            .Include(d => d.User)
            .Include(d => d.Availabilities)
            .FirstOrDefaultAsync(d => d.Id == req.DoctorId, ct);
        if (doctor is null)
            return ApiResponse<AppointmentDto>.Fail("Doctor not found.");

        var startUtc = DateTime.SpecifyKind(req.StartTime, DateTimeKind.Utc);
        var endUtc = startUtc.AddMinutes(doctor.ConsultationDurationMinutes);

        // 1. Check doctor weekly availability
        var dayOfWeek = startUtc.DayOfWeek;
        var startTimeOnly = TimeOnly.FromDateTime(startUtc);
        var endTimeOnly = TimeOnly.FromDateTime(endUtc);

        var isAvailable = doctor.Availabilities
            .Where(a => a.DeletedAt == null && a.IsActive && a.DayOfWeek == dayOfWeek)
            .Any(a => a.StartTime <= startTimeOnly && a.EndTime >= endTimeOnly);

        if (!isAvailable)
            return ApiResponse<AppointmentDto>.Fail("Doctor is not available at the requested time.");

        // 2. Double-booking protection – check for overlapping appointments
        var hasConflict = await _db.Appointments
            .AnyAsync(a =>
                a.DoctorId == req.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                a.StartTime < endUtc &&
                a.EndTime > startUtc, ct);

        if (hasConflict)
            return ApiResponse<AppointmentDto>.Fail("This time slot is already booked. Please choose another time.");

        // 3. Create appointment
        var appointment = new Appointment
        {
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
            StartTime = startUtc,
            EndTime = endUtc,
            Status = AppointmentStatus.Scheduled,
            Reason = req.Reason
        };

        _db.Appointments.Add(appointment);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<AppointmentDto>.Fail("A concurrent booking conflict occurred. Please try again.");
        }
        catch (DbUpdateException)
        {
            // Unique index / constraint violation fallback
            return ApiResponse<AppointmentDto>.Fail("This time slot is no longer available.");
        }

        var dto = MapToDto(appointment, patient, doctor);
        return ApiResponse<AppointmentDto>.Ok(dto, "Appointment booked successfully.");
    }

    private static AppointmentDto MapToDto(Appointment a, Patient p, Doctor d) =>
        new(a.Id, a.PatientId, p.User.FullName, a.DoctorId, d.User.FullName, d.Specialty,
            a.StartTime, a.EndTime, a.Status, a.Reason, a.Notes, a.CancellationReason, a.CreatedAt);
}
