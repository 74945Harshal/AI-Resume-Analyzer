namespace AIResumeAnalyzer.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation Successful")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResult(string message)
        => new() { Success = false, Message = message, Data = default };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ApiResponse SuccessResult(string message = "Operation Successful")
        => new() { Success = true, Message = message };

    public static ApiResponse FailureResult(string message)
        => new() { Success = false, Message = message };
}
