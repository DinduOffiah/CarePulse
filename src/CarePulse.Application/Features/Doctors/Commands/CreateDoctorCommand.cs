using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Doctors.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Doctors.Commands;

public record CreateDoctorCommand(CreateDoctorRequest Request) : IRequest<ApiResponse<DoctorDto>>;

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, ApiResponse<DoctorDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateDoctorCommandHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ApiResponse<DoctorDto>> Handle(CreateDoctorCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var user = await _userManager.FindByIdAsync(req.UserId.ToString());
        if (user is null || user.DeletedAt is not null)
            return ApiResponse<DoctorDto>.Fail("User not found.");

        if (await _db.Doctors.AnyAsync(d => d.UserId == req.UserId, ct))
            return ApiResponse<DoctorDto>.Fail("This user is already registered as a doctor.");

        var doctor = new Doctor
        {
            UserId = req.UserId,
            Specialty = req.Specialty.Trim(),
            LicenseNumber = req.LicenseNumber,
            Bio = req.Bio,
            ConsultationDurationMinutes = req.ConsultationDurationMinutes,
            ConsultationFee = req.ConsultationFee
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(ct);

        var dto = MapToDto(doctor, user, Array.Empty<AvailabilityDto>());
        return ApiResponse<DoctorDto>.Ok(dto, "Doctor created successfully.");
    }

    private static DoctorDto MapToDto(Doctor d, ApplicationUser u, IReadOnlyList<AvailabilityDto> avails) =>
        new(d.Id, d.UserId, u.Email!, u.FirstName, u.LastName, u.FullName,
            d.Specialty, d.LicenseNumber, d.Bio, d.ConsultationDurationMinutes,
            d.ConsultationFee, d.CreatedAt, avails);
}
