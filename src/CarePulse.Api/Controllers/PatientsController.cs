using CarePulse.Application.Features.Patients.Commands;
using CarePulse.Application.Features.Patients.DTOs;
using CarePulse.Application.Features.Patients.Queries;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a patient profile for an existing user.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreatePatientCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get patient by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientByIdQuery(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>List patients with pagination and optional search.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPatientsQuery(page, pageSize, search), ct);
        return Ok(result);
    }

    /// <summary>Update patient profile.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdatePatientCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Soft-delete a patient.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeletePatientCommand(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
