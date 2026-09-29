using CarePulse.Application.Common.Interfaces;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Patients.Commands;

public record DeletePatientCommand(Guid Id) : IRequest<ApiResponse<object>>;

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _db;

    public DeletePatientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<object>> Handle(DeletePatientCommand command, CancellationToken ct)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == command.Id, ct);
        if (patient is null)
            return ApiResponse<object>.Fail("Patient not found.");

        patient.DeletedAt = DateTime.UtcNow;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ApiResponse<object>.Ok(new { }, "Patient deleted successfully.");
    }
}
