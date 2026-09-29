using CarePulse.Application.Features.Billing.Commands;
using CarePulse.Application.Features.Billing.DTOs;
using CarePulse.Application.Features.Billing.Queries;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BillingController(IMediator mediator) => _mediator = mediator;

    /// <summary>Generate an invoice for a completed/checked-in appointment.</summary>
    [HttpPost("invoices")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateInvoiceCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get invoice by ID.</summary>
    [HttpGet("invoices/{id:guid}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>List invoices with optional status / patient filter.</summary>
    [HttpGet("invoices")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? patientId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInvoicesQuery(page, pageSize, status, patientId), ct);
        return Ok(result);
    }

    /// <summary>Mark an invoice as paid.</summary>
    [HttpPost("invoices/{id:guid}/pay")]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkInvoicePaidRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new MarkInvoicePaidCommand(id, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
