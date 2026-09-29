using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Doctors.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Doctors.Commands;

public record SetAvailabilityCommand(Guid DoctorId, List<SetAvailabilityRequest> Slots)
    : IRequest<ApiResponse<IReadOnlyList<AvailabilityDto>>>;

public class SetAvailabilityCommandHandler
    : IRequestHandler<SetAvailabilityCommand, ApiResponse<IReadOnlyList<AvailabilityDto>>>
{
    private readonly IApplicationDbContext _db;

    public SetAvailabilityCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<AvailabilityDto>>> Handle(
        SetAvailabilityCommand command, CancellationToken ct)
    {
        var doctor = await _db.Doctors
            .Include(d => d.Availabilities)
            .FirstOrDefaultAsync(d => d.Id == command.DoctorId, ct);

        if (doctor is null)
            return ApiResponse<IReadOnlyList<AvailabilityDto>>.Fail("Doctor not found.");

        // Soft-delete existing availabilities
        foreach (var existing in doctor.Availabilities.Where(a => a.DeletedAt == null))
        {
            existing.DeletedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        var newSlots = new List<DoctorAvailability>();
        foreach (var slot in command.Slots)
        {
            if (slot.StartTime >= slot.EndTime)
                return ApiResponse<IReadOnlyList<AvailabilityDto>>.Fail(
                    $"Invalid time range for {slot.DayOfWeek}: start must be before end.");

            var availability = new DoctorAvailability
            {
                DoctorId = command.DoctorId,
                DayOfWeek = slot.DayOfWeek,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsActive = slot.IsActive
            };
            newSlots.Add(availability);
            _db.DoctorAvailabilities.Add(availability);
        }

        await _db.SaveChangesAsync(ct);

        var dtos = newSlots
            .Select(a => new AvailabilityDto(a.Id, a.DayOfWeek, a.StartTime, a.EndTime, a.IsActive))
            .ToList();

        return ApiResponse<IReadOnlyList<AvailabilityDto>>.Ok(dtos, "Availability updated successfully.");
    }
}
