using CarePulse.Domain.Enums;

namespace CarePulse.Application.Features.Appointments.DTOs;

public record BookAppointmentRequest(
    Guid PatientId,
    Guid DoctorId,
    DateTime StartTime,
    string? Reason
);

public record RescheduleAppointmentRequest(
    DateTime NewStartTime
);

public record CancelAppointmentRequest(
    string? CancellationReason
);

public record UpdateAppointmentStatusRequest(
    AppointmentStatus Status
);

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    string DoctorSpecialty,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt
);

public record AppointmentListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Reason
);

public record AvailableSlotDto(
    DateTime StartTime,
    DateTime EndTime
);
