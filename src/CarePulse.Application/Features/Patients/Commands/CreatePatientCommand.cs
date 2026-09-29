using CarePulse.Application.Features.Patients.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Application.Common.Interfaces;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Patients.Commands;

public record CreatePatientCommand(CreatePatientRequest Request) : IRequest<ApiResponse<PatientDto>>;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, ApiResponse<PatientDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreatePatientCommandHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ApiResponse<PatientDto>> Handle(CreatePatientCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var user = await _userManager.FindByIdAsync(req.UserId.ToString());
        if (user is null || user.DeletedAt is not null)
            return ApiResponse<PatientDto>.Fail("User not found.");

        var alreadyPatient = await _db.Patients.AnyAsync(p => p.UserId == req.UserId, ct);
        if (alreadyPatient)
            return ApiResponse<PatientDto>.Fail("This user is already registered as a patient.");

        var patient = new Patient
        {
            UserId = req.UserId,
            DateOfBirth = req.DateOfBirth,
            Gender = req.Gender,
            BloodGroup = req.BloodGroup,
            Address = req.Address,
            EmergencyContactName = req.EmergencyContactName,
            EmergencyContactPhone = req.EmergencyContactPhone,
            MedicalHistorySummary = req.MedicalHistorySummary,
            Allergies = req.Allergies
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);

        var dto = new PatientDto(
            patient.Id, patient.UserId, user.Email!, user.FirstName, user.LastName, user.FullName,
            patient.DateOfBirth, patient.Gender, patient.BloodGroup, patient.Address,
            patient.EmergencyContactName, patient.EmergencyContactPhone,
            patient.MedicalHistorySummary, patient.Allergies, patient.CreatedAt);

        return ApiResponse<PatientDto>.Ok(dto, "Patient created successfully.");
    }
}
