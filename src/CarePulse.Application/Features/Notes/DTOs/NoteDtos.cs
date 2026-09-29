namespace CarePulse.Application.Features.Notes.DTOs;

public record CreateConsultationNoteRequest(
    Guid AppointmentId,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string? Prescriptions
);

public record UpdateConsultationNoteRequest(
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string? Prescriptions
);

public record ConsultationNoteDto(
    Guid Id,
    Guid AppointmentId,
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string? Prescriptions,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
