using CarePulse.Domain.Common;

namespace CarePulse.Domain.Entities;

public class Doctor : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Specialty { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? Bio { get; set; }
    public int ConsultationDurationMinutes { get; set; } = 30;
    public decimal ConsultationFee { get; set; }

    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
