using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Notes.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Notes.Commands;

public record UpdateConsultationNoteCommand(Guid Id, UpdateConsultationNoteRequest Request)
    : IRequest<ApiResponse<ConsultationNoteDto>>;

public class UpdateConsultationNoteCommandHandler
    : IRequestHandler<UpdateConsultationNoteCommand, ApiResponse<ConsultationNoteDto>>
{
    private readonly IApplicationDbContext _db;

    public UpdateConsultationNoteCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<ConsultationNoteDto>> Handle(
        UpdateConsultationNoteCommand command, CancellationToken ct)
    {
        var note = await _db.ConsultationNotes.FirstOrDefaultAsync(n => n.Id == command.Id, ct);
        if (note is null)
            return ApiResponse<ConsultationNoteDto>.Fail("Consultation note not found.");

        var req = command.Request;
        note.Subjective = req.Subjective.Trim();
        note.Objective = req.Objective.Trim();
        note.Assessment = req.Assessment.Trim();
        note.Plan = req.Plan.Trim();
        note.Prescriptions = req.Prescriptions?.Trim();
        note.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var dto = new ConsultationNoteDto(
            note.Id, note.AppointmentId, note.Subjective, note.Objective,
            note.Assessment, note.Plan, note.Prescriptions, note.CreatedAt, note.UpdatedAt);

        return ApiResponse<ConsultationNoteDto>.Ok(dto, "Consultation note updated successfully.");
    }
}
