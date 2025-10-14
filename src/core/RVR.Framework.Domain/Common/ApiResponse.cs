namespace RVR.Framework.Domain.Common;

/// <summary>
/// Réponse API standardisée
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? CorrelationId { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

/// <summary>
/// Détails d'erreur pour le développement
/// </summary>
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? CorrelationId { get; set; }
}
