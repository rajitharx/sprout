namespace Sprout.Api.Models;

public record ApiError(
    string Code,
    string Message,
    Dictionary<string, string[]>? ValidationErrors = null
);

public record ApiErrorResponse(
    ApiError Error
);

public static class ApiErrors
{
    public static ApiErrorResponse ValidationError(Dictionary<string, string[]> errors)
        => new(new ApiError("VALIDATION_ERROR", "One or more validation errors occurred.", errors));

    public static ApiErrorResponse ValidationError(string field, string message)
        => new(new ApiError("VALIDATION_ERROR", "One or more validation errors occurred.",
            new Dictionary<string, string[]> { { field, [message] } }));

    public static ApiErrorResponse NotFound(string resource)
        => new(new ApiError("NOT_FOUND", $"{resource} not found."));

    public static ApiErrorResponse ConflictError(string message)
        => new(new ApiError("CONFLICT", message));

    public static ApiErrorResponse InternalError(string message = "An unexpected error occurred.")
        => new(new ApiError("INTERNAL_ERROR", message));
}
