namespace CarePulse.Application.Features.Billing.DTOs;

public record GenerateInvoiceRequest(
    Guid AppointmentId,
    decimal? TaxRate = null   // optional override, default 0
);

public record MarkInvoicePaidRequest(
    string? PaymentReference
);

public record InvoiceDto(
    Guid Id,
    Guid AppointmentId,
    decimal Amount,
    decimal Tax,
    decimal TotalAmount,
    string Status,
    DateTime? PaidAt,
    string? PaymentReference,
    DateTime CreatedAt
);

public record InvoiceListItemDto(
    Guid Id,
    Guid AppointmentId,
    string PatientName,
    string DoctorName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? PaidAt
);
