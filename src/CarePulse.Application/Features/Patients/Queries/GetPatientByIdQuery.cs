using CarePulse.Application.Features.Patients.DTOs;
using CarePulse.Application.Common.Interfaces;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Patients.Queries;

public record GetPatientByIdQuery(Guid Id) : IRequest<ApiResponse<PatientDto>>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, ApiResponse<PatientDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PatientDto>> Handle(GetPatientByIdQuery query, CancellationToken ct)
    {
        var patient = await _db.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, ct);

        if (patient is null)
            return ApiResponse<PatientDto>.Fail("Patient not found.");

        var dto = new PatientDto(
            patient.Id, patient.UserId, patient.User.Email!, patient.User.FirstName, patient.User.LastName,
            patient.User.FullName, patient.DateOfBirth, patient.Gender, patient.BloodGroup, patient.Address,
            patient.EmergencyContactName, patient.EmergencyContactPhone,
            patient.MedicalHistorySummary, patient.Allergies, patient.CreatedAt);

        return ApiResponse<PatientDto>.Ok(dto);
    }
}
