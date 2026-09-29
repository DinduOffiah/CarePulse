using CarePulse.Application.Features.Appointments.Commands;
using CarePulse.Application.Features.Appointments.DTOs;
using CarePulse.Application.Features.Appointments.Queries;
using CarePulse.Domain.Enums;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Book a new appointment (double-booking protected).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new BookAppointmentCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get appointment by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAppointmentByIdQuery(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>List appointments with filters and pagination.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] AppointmentStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAppointmentsQuery(page, pageSize, patientId, doctorId, status, from, to), ct);
        return Ok(result);
    }

    /// <summary>Get available time slots for a doctor on a given date.</summary>
    [HttpGet("available-slots")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailableSlotDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid doctorId,
        [FromQuery] DateOnly date,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAvailableSlotsQuery(doctorId, date), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Reschedule an existing appointment.</summary>
    [HttpPut("{id:guid}/reschedule")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reschedule(
        Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RescheduleAppointmentCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancel an appointment.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Receptionist,Patient,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(
        Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update appointment status (status machine enforced).</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateAppointmentStatusCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
