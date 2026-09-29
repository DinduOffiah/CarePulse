using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Notes.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Notes.Commands;

public record CreateConsultationNoteCommand(CreateConsultationNoteRequest Request)
    : IRequest<ApiResponse<ConsultationNoteDto>>;

public class CreateConsultationNoteCommandHandler
    : IRequestHandler<CreateConsultationNoteCommand, ApiResponse<ConsultationNoteDto>>
{
    private readonly IApplicationDbContext _db;

    public CreateConsultationNoteCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<ConsultationNoteDto>> Handle(
        CreateConsultationNoteCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var appointment = await _db.Appointments
            .Include(a => a.ConsultationNote)
            .FirstOrDefaultAsync(a => a.Id == req.AppointmentId, ct);

        if (appointment is null)
            return ApiResponse<ConsultationNoteDto>.Fail("Appointment not found.");

        if (appointment.Status is not (AppointmentStatus.InProgress or AppointmentStatus.Completed or AppointmentStatus.CheckedIn))
            return ApiResponse<ConsultationNoteDto>.Fail(
                "Consultation notes can only be added when the appointment is CheckedIn, InProgress, or Completed.");

        if (appointment.ConsultationNote is not null)
            return ApiResponse<ConsultationNoteDto>.Fail("A consultation note already exists for this appointment.");

        var note = new ConsultationNote
        {
            AppointmentId = req.AppointmentId,
            Subjective = req.Subjective.Trim(),
            Objective = req.Objective.Trim(),
            Assessment = req.Assessment.Trim(),
            Plan = req.Plan.Trim(),
            Prescriptions = req.Prescriptions?.Trim()
        };

        _db.ConsultationNotes.Add(note);
        await _db.SaveChangesAsync(ct);

        var dto = new ConsultationNoteDto(
            note.Id, note.AppointmentId, note.Subjective, note.Objective,
            note.Assessment, note.Plan, note.Prescriptions, note.CreatedAt, note.UpdatedAt);

        return ApiResponse<ConsultationNoteDto>.Ok(dto, "Consultation note created successfully.");
    }
}
