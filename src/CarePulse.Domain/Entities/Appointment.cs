using CarePulse.Domain.Common;
using CarePulse.Domain.Enums;

namespace CarePulse.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public uint RowVersion { get; set; } // Optimistic concurrency

    public ConsultationNote? ConsultationNote { get; set; }
    public Invoice? Invoice { get; set; }
}
