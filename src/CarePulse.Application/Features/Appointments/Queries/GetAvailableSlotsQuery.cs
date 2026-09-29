using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Queries;

public record GetAvailableSlotsQuery(
    Guid DoctorId,
    DateOnly Date
) : IRequest<ApiResponse<IReadOnlyList<AvailableSlotDto>>>;

public class GetAvailableSlotsQueryHandler
    : IRequestHandler<GetAvailableSlotsQuery, ApiResponse<IReadOnlyList<AvailableSlotDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAvailableSlotsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<AvailableSlotDto>>> Handle(
        GetAvailableSlotsQuery query, CancellationToken ct)
    {
        var doctor = await _db.Doctors
            .Include(d => d.Availabilities)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == query.DoctorId, ct);

        if (doctor is null)
            return ApiResponse<IReadOnlyList<AvailableSlotDto>>.Fail("Doctor not found.");

        var dayOfWeek = query.Date.DayOfWeek;
        var availWindows = doctor.Availabilities
            .Where(a => a.DeletedAt == null && a.IsActive && a.DayOfWeek == dayOfWeek)
            .ToList();

        if (availWindows.Count == 0)
            return ApiResponse<IReadOnlyList<AvailableSlotDto>>.Ok(Array.Empty<AvailableSlotDto>(),
                "Doctor has no availability on this day.");

        var dayStart = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var existing = await _db.Appointments
            .Where(a =>
                a.DoctorId == query.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                a.StartTime >= dayStart &&
                a.StartTime < dayEnd)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync(ct);

        var duration = TimeSpan.FromMinutes(doctor.ConsultationDurationMinutes);
        var slots = new List<AvailableSlotDto>();

        foreach (var window in availWindows)
        {
            var cursor = query.Date.ToDateTime(window.StartTime, DateTimeKind.Utc);
            var windowEnd = query.Date.ToDateTime(window.EndTime, DateTimeKind.Utc);

            while (cursor.Add(duration) <= windowEnd)
            {
                var slotEnd = cursor.Add(duration);

                // Skip past slots
                if (cursor > DateTime.UtcNow)
                {
                    var overlaps = existing.Any(e => e.StartTime < slotEnd && e.EndTime > cursor);
                    if (!overlaps)
                    {
                        slots.Add(new AvailableSlotDto(cursor, slotEnd));
                    }
                }

                cursor = cursor.Add(duration);
            }
        }

        return ApiResponse<IReadOnlyList<AvailableSlotDto>>.Ok(slots);
    }
}
