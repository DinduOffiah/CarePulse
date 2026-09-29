using CarePulse.Application.Common.Models;
using CarePulse.Application.Features.Patients.DTOs;
using CarePulse.Application.Common.Interfaces;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Patients.Queries;

public record GetPatientsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<ApiResponse<PagedResult<PatientListItemDto>>>;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, ApiResponse<PagedResult<PatientListItemDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PagedResult<PatientListItemDto>>> Handle(GetPatientsQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(p =>
                p.User.FirstName.ToLower().Contains(term) ||
                p.User.LastName.ToLower().Contains(term) ||
                p.User.Email!.ToLower().Contains(term));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientListItemDto(
                p.Id, p.UserId, p.User.FullName, p.User.Email!, p.Gender, p.DateOfBirth, p.CreatedAt))
            .ToListAsync(ct);

        var result = new PagedResult<PatientListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return ApiResponse<PagedResult<PatientListItemDto>>.Ok(result);
    }
}
