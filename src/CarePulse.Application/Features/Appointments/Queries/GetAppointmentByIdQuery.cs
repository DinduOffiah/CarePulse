using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Queries;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<ApiResponse<AppointmentDto>>;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, ApiResponse<AppointmentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAppointmentByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<AppointmentDto>> Handle(GetAppointmentByIdQuery query, CancellationToken ct)
    {
        var a = await _db.Appointments
            .Include(x => x.Patient).ThenInclude(p => p.User)
            .Include(x => x.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

        if (a is null)
            return ApiResponse<AppointmentDto>.Fail("Appointment not found.");

        var dto = new AppointmentDto(
            a.Id, a.PatientId, a.Patient.User.FullName,
            a.DoctorId, a.Doctor.User.FullName, a.Doctor.Specialty,
            a.StartTime, a.EndTime, a.Status,
            a.Reason, a.Notes, a.CancellationReason, a.CreatedAt);

        return ApiResponse<AppointmentDto>.Ok(dto);
    }
}
