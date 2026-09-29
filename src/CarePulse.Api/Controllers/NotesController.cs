using CarePulse.Application.Features.Notes.Commands;
using CarePulse.Application.Features.Notes.DTOs;
using CarePulse.Application.Features.Notes.Queries;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a SOAP consultation note for an appointment.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<ConsultationNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateConsultationNoteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateConsultationNoteCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get consultation note by appointment ID.</summary>
    [HttpGet("by-appointment/{appointmentId:guid}")]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    [ProducesResponseType(typeof(ApiResponse<ConsultationNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAppointment(Guid appointmentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetConsultationNoteByAppointmentQuery(appointmentId), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update an existing consultation note.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<ConsultationNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConsultationNoteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateConsultationNoteCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
