using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Doctors.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Doctors.Queries;

public record GetDoctorByIdQuery(Guid Id) : IRequest<ApiResponse<DoctorDto>>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, ApiResponse<DoctorDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDoctorByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<DoctorDto>> Handle(GetDoctorByIdQuery query, CancellationToken ct)
    {
        var doctor = await _db.Doctors
            .Include(d => d.User)
            .Include(d => d.Availabilities)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == query.Id, ct);

        if (doctor is null)
            return ApiResponse<DoctorDto>.Fail("Doctor not found.");

        var avails = doctor.Availabilities
            .Where(a => a.DeletedAt == null)
            .Select(a => new AvailabilityDto(a.Id, a.DayOfWeek, a.StartTime, a.EndTime, a.IsActive))
            .ToList();

        var dto = new DoctorDto(
            doctor.Id, doctor.UserId, doctor.User.Email!, doctor.User.FirstName, doctor.User.LastName,
            doctor.User.FullName, doctor.Specialty, doctor.LicenseNumber, doctor.Bio,
            doctor.ConsultationDurationMinutes, doctor.ConsultationFee, doctor.CreatedAt, avails);

        return ApiResponse<DoctorDto>.Ok(dto);
    }
}
