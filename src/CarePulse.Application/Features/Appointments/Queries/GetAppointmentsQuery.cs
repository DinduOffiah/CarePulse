using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Common.Models;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Appointments.Queries;

public record GetAppointmentsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? PatientId = null,
    Guid? DoctorId = null,
    AppointmentStatus? Status = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<ApiResponse<PagedResult<AppointmentListItemDto>>>;

public class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, ApiResponse<PagedResult<AppointmentListItemDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAppointmentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PagedResult<AppointmentListItemDto>>> Handle(
        GetAppointmentsQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .AsQueryable();

        if (query.PatientId.HasValue)
            q = q.Where(a => a.PatientId == query.PatientId.Value);

        if (query.DoctorId.HasValue)
            q = q.Where(a => a.DoctorId == query.DoctorId.Value);

        if (query.Status.HasValue)
            q = q.Where(a => a.Status == query.Status.Value);

        if (query.From.HasValue)
            q = q.Where(a => a.StartTime >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(a => a.StartTime <= query.To.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentListItemDto(
                a.Id, a.PatientId, a.Patient.User.FullName,
                a.DoctorId, a.Doctor.User.FullName,
                a.StartTime, a.EndTime, a.Status, a.Reason))
            .ToListAsync(ct);

        var result = new PagedResult<AppointmentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return ApiResponse<PagedResult<AppointmentListItemDto>>.Ok(result);
    }
}
