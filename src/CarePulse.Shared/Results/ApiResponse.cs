namespace CarePulse.Shared.Results;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Meta { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;

    public static ApiResponse<T> Ok(T data, string message = "Success", object? meta = null, string? requestId = null)
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Meta = meta,
            RequestId = requestId ?? string.Empty
        };

    public static ApiResponse<T> Fail(string message, string? requestId = null)
        => new()
        {
            Success = false,
            Message = message,
            RequestId = requestId ?? string.Empty
        };

    public static ApiResponse<T> Fail(string message, List<string> errors, string? requestId = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors,
            RequestId = requestId ?? string.Empty
        };
}

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;

    public static ApiErrorResponse Create(string message, List<string>? errors = null, string? requestId = null)
        => new()
        {
            Message = message,
            Errors = errors ?? new List<string>(),
            RequestId = requestId ?? string.Empty
        };
}
