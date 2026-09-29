using CarePulse.Application.Features.Doctors.Commands;
using CarePulse.Application.Features.Doctors.DTOs;
using CarePulse.Application.Features.Doctors.Queries;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a doctor profile for an existing user.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDoctorCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get doctor by ID (includes availability).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDoctorByIdQuery(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>List doctors with pagination, specialty filter and search.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? specialty = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDoctorsQuery(page, pageSize, specialty, search), ct);
        return Ok(result);
    }

    /// <summary>Update doctor profile.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateDoctorCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Soft-delete a doctor.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteDoctorCommand(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Replace the doctor's weekly availability schedule.</summary>
    [HttpPut("{id:guid}/availability")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailabilityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAvailability(
        Guid id,
        [FromBody] List<SetAvailabilityRequest> slots,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new SetAvailabilityCommand(id, slots), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
