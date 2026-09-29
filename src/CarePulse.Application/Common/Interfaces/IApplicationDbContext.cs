using CarePulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<ConsultationNote> ConsultationNotes { get; }
    DbSet<Invoice> Invoices { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
