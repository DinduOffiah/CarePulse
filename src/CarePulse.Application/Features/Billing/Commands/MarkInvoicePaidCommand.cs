using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Billing.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Billing.Commands;

public record MarkInvoicePaidCommand(Guid Id, MarkInvoicePaidRequest Request)
    : IRequest<ApiResponse<InvoiceDto>>;

public class MarkInvoicePaidCommandHandler : IRequestHandler<MarkInvoicePaidCommand, ApiResponse<InvoiceDto>>
{
    private readonly IApplicationDbContext _db;

    public MarkInvoicePaidCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<InvoiceDto>> Handle(MarkInvoicePaidCommand command, CancellationToken ct)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == command.Id, ct);
        if (invoice is null)
            return ApiResponse<InvoiceDto>.Fail("Invoice not found.");

        if (invoice.Status == "Paid")
            return ApiResponse<InvoiceDto>.Fail("Invoice is already marked as paid.");

        if (invoice.Status is "Cancelled" or "Refunded")
            return ApiResponse<InvoiceDto>.Fail($"Cannot mark a {invoice.Status.ToLower()} invoice as paid.");

        invoice.Status = "Paid";
        invoice.PaidAt = DateTime.UtcNow;
        invoice.PaymentReference = command.Request.PaymentReference;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var dto = new InvoiceDto(
            invoice.Id, invoice.AppointmentId, invoice.Amount, invoice.Tax,
            invoice.TotalAmount, invoice.Status, invoice.PaidAt, invoice.PaymentReference, invoice.CreatedAt);

        return ApiResponse<InvoiceDto>.Ok(dto, "Invoice marked as paid.");
    }
}
