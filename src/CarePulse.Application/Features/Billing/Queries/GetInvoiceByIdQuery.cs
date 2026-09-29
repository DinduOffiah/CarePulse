using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Billing.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Billing.Queries;

public record GetInvoiceByIdQuery(Guid Id) : IRequest<ApiResponse<InvoiceDto>>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, ApiResponse<InvoiceDto>>
{
    private readonly IApplicationDbContext _db;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == query.Id, ct);

        if (invoice is null)
            return ApiResponse<InvoiceDto>.Fail("Invoice not found.");

        var dto = new InvoiceDto(
            invoice.Id, invoice.AppointmentId, invoice.Amount, invoice.Tax,
            invoice.TotalAmount, invoice.Status, invoice.PaidAt, invoice.PaymentReference, invoice.CreatedAt);

        return ApiResponse<InvoiceDto>.Ok(dto);
    }
}
