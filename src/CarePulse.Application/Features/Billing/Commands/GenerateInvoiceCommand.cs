using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Billing.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Billing.Commands;

public record GenerateInvoiceCommand(GenerateInvoiceRequest Request) : IRequest<ApiResponse<InvoiceDto>>;

public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, ApiResponse<InvoiceDto>>
{
    private readonly IApplicationDbContext _db;
    private const decimal DefaultTaxRate = 0.0m;

    public GenerateInvoiceCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<InvoiceDto>> Handle(GenerateInvoiceCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var appointment = await _db.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Invoice)
            .FirstOrDefaultAsync(a => a.Id == req.AppointmentId, ct);

        if (appointment is null)
            return ApiResponse<InvoiceDto>.Fail("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            return ApiResponse<InvoiceDto>.Fail("Cannot generate invoice for cancelled or no-show appointments.");

        if (appointment.Invoice is not null)
            return ApiResponse<InvoiceDto>.Fail("An invoice already exists for this appointment.");

        var amount = appointment.Doctor.ConsultationFee;
        var taxRate = req.TaxRate ?? DefaultTaxRate;
        var tax = Math.Round(amount * taxRate, 2);
        var total = amount + tax;

        var invoice = new Invoice
        {
            AppointmentId = req.AppointmentId,
            Amount = amount,
            Tax = tax,
            TotalAmount = total,
            Status = "Pending"
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        var dto = new InvoiceDto(
            invoice.Id, invoice.AppointmentId, invoice.Amount, invoice.Tax,
            invoice.TotalAmount, invoice.Status, invoice.PaidAt, invoice.PaymentReference, invoice.CreatedAt);

        return ApiResponse<InvoiceDto>.Ok(dto, "Invoice generated successfully.");
    }
}
