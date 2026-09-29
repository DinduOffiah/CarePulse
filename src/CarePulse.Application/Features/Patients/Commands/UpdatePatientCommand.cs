using CarePulse.Application.Features.Patients.DTOs;
using CarePulse.Application.Common.Interfaces;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Patients.Commands;

public record UpdatePatientCommand(Guid Id, UpdatePatientRequest Request) : IRequest<ApiResponse<PatientDto>>;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, ApiResponse<PatientDto>>
{
    private readonly IApplicationDbContext _db;

    public UpdatePatientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PatientDto>> Handle(UpdatePatientCommand command, CancellationToken ct)
    {
        var patient = await _db.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (patient is null)
            return ApiResponse<PatientDto>.Fail("Patient not found.");

        var req = command.Request;
        patient.DateOfBirth = req.DateOfBirth ?? patient.DateOfBirth;
        patient.Gender = req.Gender ?? patient.Gender;
        patient.BloodGroup = req.BloodGroup ?? patient.BloodGroup;
        patient.Address = req.Address ?? patient.Address;
        patient.EmergencyContactName = req.EmergencyContactName ?? patient.EmergencyContactName;
        patient.EmergencyContactPhone = req.EmergencyContactPhone ?? patient.EmergencyContactPhone;
        patient.MedicalHistorySummary = req.MedicalHistorySummary ?? patient.MedicalHistorySummary;
        patient.Allergies = req.Allergies ?? patient.Allergies;
        patient.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var dto = new PatientDto(
            patient.Id, patient.UserId, patient.User.Email!, patient.User.FirstName, patient.User.LastName,
            patient.User.FullName, patient.DateOfBirth, patient.Gender, patient.BloodGroup, patient.Address,
            patient.EmergencyContactName, patient.EmergencyContactPhone,
            patient.MedicalHistorySummary, patient.Allergies, patient.CreatedAt);

        return ApiResponse<PatientDto>.Ok(dto, "Patient updated successfully.");
    }
}
