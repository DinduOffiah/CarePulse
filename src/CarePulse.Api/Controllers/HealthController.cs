using CarePulse.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var requestId = HttpContext.TraceIdentifier;
        return Ok(ApiResponse<object>.Ok(
            new { status = "Healthy", version = "1.0.0", service = "CarePulse API" },
            "CarePulse API is running",
            requestId: requestId));
    }
}
