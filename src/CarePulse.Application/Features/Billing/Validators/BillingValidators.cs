using CarePulse.Application.Features.Billing.DTOs;
using FluentValidation;

namespace CarePulse.Application.Features.Billing.Validators;

public class GenerateInvoiceRequestValidator : AbstractValidator<GenerateInvoiceRequest>
{
    public GenerateInvoiceRequestValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1)
            .When(x => x.TaxRate.HasValue)
            .WithMessage("Tax rate must be between 0 and 1 (e.g. 0.075 for 7.5%).");
    }
}

public class MarkInvoicePaidRequestValidator : AbstractValidator<MarkInvoicePaidRequest>
{
    public MarkInvoicePaidRequestValidator()
    {
        RuleFor(x => x.PaymentReference).MaximumLength(100).When(x => x.PaymentReference is not null);
    }
}
