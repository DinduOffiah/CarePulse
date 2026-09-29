using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Common.Models;
using CarePulse.Application.Features.Billing.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Billing.Queries;

public record GetInvoicesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? PatientId = null
) : IRequest<ApiResponse<PagedResult<InvoiceListItemDto>>>;

public class GetInvoicesQueryHandler
    : IRequestHandler<GetInvoicesQuery, ApiResponse<PagedResult<InvoiceListItemDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetInvoicesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PagedResult<InvoiceListItemDto>>> Handle(
        GetInvoicesQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Invoices
            .Include(i => i.Appointment).ThenInclude(a => a.Patient).ThenInclude(p => p.User)
            .Include(i => i.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(i => i.Status == query.Status);

        if (query.PatientId.HasValue)
            q = q.Where(i => i.Appointment.PatientId == query.PatientId.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceListItemDto(
                i.Id,
                i.AppointmentId,
                i.Appointment.Patient.User.FullName,
                i.Appointment.Doctor.User.FullName,
                i.TotalAmount,
                i.Status,
                i.CreatedAt,
                i.PaidAt))
            .ToListAsync(ct);

        var result = new PagedResult<InvoiceListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        return ApiResponse<PagedResult<InvoiceListItemDto>>.Ok(result);
    }
}
