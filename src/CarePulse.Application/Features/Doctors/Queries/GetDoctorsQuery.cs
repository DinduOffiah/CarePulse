using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Common.Models;
using CarePulse.Application.Features.Doctors.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Doctors.Queries;

public record GetDoctorsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Specialty = null,
    string? Search = null
) : IRequest<ApiResponse<PagedResult<DoctorListItemDto>>>;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, ApiResponse<PagedResult<DoctorListItemDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetDoctorsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PagedResult<DoctorListItemDto>>> Handle(GetDoctorsQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Specialty))
        {
            var specialty = query.Specialty.Trim().ToLower();
            q = q.Where(d => d.Specialty.ToLower().Contains(specialty));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(d =>
                d.User.FirstName.ToLower().Contains(term) ||
                d.User.LastName.ToLower().Contains(term) ||
                d.User.Email!.ToLower().Contains(term) ||
                d.Specialty.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(d => d.User.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DoctorListItemDto(
                d.Id, d.UserId, d.User.FullName, d.User.Email!, d.Specialty,
                d.ConsultationFee, d.ConsultationDurationMinutes))
            .ToListAsync(ct);

        var result = new PagedResult<DoctorListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return ApiResponse<PagedResult<DoctorListItemDto>>.Ok(result);
    }
}
