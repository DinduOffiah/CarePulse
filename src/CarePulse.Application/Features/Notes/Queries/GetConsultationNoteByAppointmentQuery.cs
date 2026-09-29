using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Notes.DTOs;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.Application.Features.Notes.Queries;

public record GetConsultationNoteByAppointmentQuery(Guid AppointmentId)
    : IRequest<ApiResponse<ConsultationNoteDto>>;

public class GetConsultationNoteByAppointmentQueryHandler
    : IRequestHandler<GetConsultationNoteByAppointmentQuery, ApiResponse<ConsultationNoteDto>>
{
    private readonly IApplicationDbContext _db;

    public GetConsultationNoteByAppointmentQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<ConsultationNoteDto>> Handle(
        GetConsultationNoteByAppointmentQuery query, CancellationToken ct)
    {
        var note = await _db.ConsultationNotes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.AppointmentId == query.AppointmentId, ct);

        if (note is null)
            return ApiResponse<ConsultationNoteDto>.Fail("Consultation note not found for this appointment.");

        var dto = new ConsultationNoteDto(
            note.Id, note.AppointmentId, note.Subjective, note.Objective,
            note.Assessment, note.Plan, note.Prescriptions, note.CreatedAt, note.UpdatedAt);

        return ApiResponse<ConsultationNoteDto>.Ok(dto);
    }
}
