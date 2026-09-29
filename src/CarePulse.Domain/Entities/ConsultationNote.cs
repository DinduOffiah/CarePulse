using CarePulse.Domain.Common;

namespace CarePulse.Domain.Entities;

public class ConsultationNote : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string Subjective { get; set; } = string.Empty; // Patient complaint
    public string Objective { get; set; } = string.Empty;  // Clinical findings
    public string Assessment { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string? Prescriptions { get; set; }
}
