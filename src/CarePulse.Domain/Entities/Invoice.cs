using CarePulse.Domain.Common;

namespace CarePulse.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled, Refunded
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
}
