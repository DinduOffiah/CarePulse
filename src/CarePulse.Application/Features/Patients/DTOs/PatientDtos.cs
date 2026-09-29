namespace CarePulse.Application.Features.Patients.DTOs;

public record CreatePatientRequest(
    Guid UserId,
    DateTime? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? MedicalHistorySummary,
    string? Allergies
);

public record UpdatePatientRequest(
    DateTime? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? MedicalHistorySummary,
    string? Allergies
);

public record PatientDto(
    Guid Id,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    DateTime? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? MedicalHistorySummary,
    string? Allergies,
    DateTime CreatedAt
);

public record PatientListItemDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Gender,
    DateTime? DateOfBirth,
    DateTime CreatedAt
);
