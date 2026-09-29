using System.Net;
using System.Text.Json;
using CarePulse.Shared.Results;

namespace CarePulse.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        _logger.LogError(exception, "Unhandled exception. RequestId: {RequestId}", requestId);

        var (statusCode, message, errors) = exception switch
        {
            ArgumentException or ArgumentNullException => (HttpStatusCode.BadRequest, exception.Message, new List<string> { exception.Message }),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access.", new List<string>()),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message, new List<string>()),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message, new List<string> { exception.Message }),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", new List<string>())
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiErrorResponse.Create(message, errors, requestId);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
