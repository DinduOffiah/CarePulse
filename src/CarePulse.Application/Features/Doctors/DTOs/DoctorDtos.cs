namespace CarePulse.Application.Features.Doctors.DTOs;

public record CreateDoctorRequest(
    Guid UserId,
    string Specialty,
    string? LicenseNumber,
    string? Bio,
    int ConsultationDurationMinutes,
    decimal ConsultationFee
);

public record UpdateDoctorRequest(
    string Specialty,
    string? LicenseNumber,
    string? Bio,
    int ConsultationDurationMinutes,
    decimal ConsultationFee
);

public record DoctorDto(
    Guid Id,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Specialty,
    string? LicenseNumber,
    string? Bio,
    int ConsultationDurationMinutes,
    decimal ConsultationFee,
    DateTime CreatedAt,
    IReadOnlyList<AvailabilityDto> Availabilities
);

public record DoctorListItemDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Specialty,
    decimal ConsultationFee,
    int ConsultationDurationMinutes
);

public record SetAvailabilityRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive = true
);

public record AvailabilityDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);
