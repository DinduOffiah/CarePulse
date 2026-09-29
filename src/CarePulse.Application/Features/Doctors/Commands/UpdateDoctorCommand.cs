using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Doctors.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Doctors.Commands;

public record UpdateDoctorCommand(Guid Id, UpdateDoctorRequest Request) : IRequest<ApiResponse<DoctorDto>>;

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, ApiResponse<DoctorDto>>
{
    private readonly IApplicationDbContext _db;

    public UpdateDoctorCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<DoctorDto>> Handle(UpdateDoctorCommand command, CancellationToken ct)
    {
        var doctor = await _db.Doctors
            .Include(d => d.User)
            .Include(d => d.Availabilities)
            .FirstOrDefaultAsync(d => d.Id == command.Id, ct);

        if (doctor is null)
            return ApiResponse<DoctorDto>.Fail("Doctor not found.");

        var req = command.Request;
        doctor.Specialty = req.Specialty.Trim();
        doctor.LicenseNumber = req.LicenseNumber;
        doctor.Bio = req.Bio;
        doctor.ConsultationDurationMinutes = req.ConsultationDurationMinutes;
        doctor.ConsultationFee = req.ConsultationFee;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var avails = doctor.Availabilities
            .Where(a => a.DeletedAt == null)
            .Select(a => new AvailabilityDto(a.Id, a.DayOfWeek, a.StartTime, a.EndTime, a.IsActive))
            .ToList();

        var dto = new DoctorDto(
            doctor.Id, doctor.UserId, doctor.User.Email!, doctor.User.FirstName, doctor.User.LastName,
            doctor.User.FullName, doctor.Specialty, doctor.LicenseNumber, doctor.Bio,
            doctor.ConsultationDurationMinutes, doctor.ConsultationFee, doctor.CreatedAt, avails);

        return ApiResponse<DoctorDto>.Ok(dto, "Doctor updated successfully.");
    }
}
