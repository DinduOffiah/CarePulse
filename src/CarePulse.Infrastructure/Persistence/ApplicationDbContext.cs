using CarePulse.Domain.Common;
using CarePulse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, CarePulse.Application.Common.Interfaces.IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ConsultationNote> ConsultationNotes => Set<ConsultationNote>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        builder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Patient>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        builder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Doctor>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Specialty).HasMaxLength(150).IsRequired();
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        builder.Entity<DoctorAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek });
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        builder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => new { e.DoctorId, e.StartTime, e.EndTime });
            entity.HasIndex(e => e.PatientId);
            entity.Property(e => e.RowVersion).IsConcurrencyToken();
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        builder.Entity<ConsultationNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.ConsultationNote)
                .HasForeignKey<ConsultationNote>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        builder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.Invoice)
                .HasForeignKey<Invoice>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
