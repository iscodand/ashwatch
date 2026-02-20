namespace AshWatch.Application.Common;

public class DefaultResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = [];

    public static DefaultResponse<T> Ok(T data, string message = "Success")
    {
        return new DefaultResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static DefaultResponse<T> Fail(string message, params string[] errors)
    {
        return new DefaultResponse<T>
        {
            Success = false,
            Message = message,
            Errors = [.. errors]
        };
    }
}
