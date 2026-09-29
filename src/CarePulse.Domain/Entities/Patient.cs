using CarePulse.Domain.Common;

namespace CarePulse.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalHistorySummary { get; set; }
    public string? Allergies { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
