using CarePulse.Application.Common.Interfaces;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Doctors.Commands;

public record DeleteDoctorCommand(Guid Id) : IRequest<ApiResponse<object>>;

public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand, ApiResponse<object>>
{
    private readonly IApplicationDbContext _db;

    public DeleteDoctorCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<object>> Handle(DeleteDoctorCommand command, CancellationToken ct)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == command.Id, ct);
        if (doctor is null)
            return ApiResponse<object>.Fail("Doctor not found.");

        doctor.DeletedAt = DateTime.UtcNow;
        doctor.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ApiResponse<object>.Ok(new { }, "Doctor deleted successfully.");
    }
}
