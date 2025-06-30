namespace LibSystem.Api.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ApiError? Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = Guid.NewGuid().ToString();
    }
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }

    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public Dictionary<string, object>? Details { get; set; }
    }

    public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public static class ApiResponseConstants
    {
        public static class SuccessMessages
        {
            public const string Retrieved = "Retrieved successfully";
            public const string Created = "Created successfully";
            public const string Updated = "Updated successfully";
            public const string Deleted = "Deleted successfully";
            public const string Authenticated = "Authenticated successfully";
            public const string OperationCompleted = "Operation completed successfully";
        }

        public static class ErrorMessages
        {
            public const string ValidationFailed = "One or more validation errors occurred";
            public const string NotFound = "The requested resource was not found";
            public const string Conflict = "The operation conflicts with the current state";
            public const string Unauthorized = "Authentication is required";
            public const string Forbidden = "You do not have permission to perform this action";
            public const string InternalError = "An internal server error occurred";
            public const string ServiceUnavailable = "The service is currently unavailable";
        }

        public static class ErrorCodes
        {
            public const string ValidationError = "VALIDATION_ERROR";
            public const string NotFound = "NOT_FOUND";
            public const string Conflict = "CONFLICT";
            public const string Unauthorized = "UNAUTHORIZED";
            public const string Forbidden = "FORBIDDEN";
            public const string InternalError = "INTERNAL_ERROR";
            public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        }
    }

    public static class ApiResponseHelper
    {
        public static ApiResponse<T> Success<T>(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? ApiResponseConstants.SuccessMessages.OperationCompleted
            };
        }

        public static ApiResponse Success(string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message ?? ApiResponseConstants.SuccessMessages.OperationCompleted
            };
        }

        public static ApiResponse<T> Error<T>(string code, string message, string errorType = "Failure")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = code,
                    Message = message,
                    Type = errorType
                }
            };
        }

        public static ApiResponse Error(string code, string message, string errorType = "Failure")
        {
            return new ApiResponse
            {
                Success = false,
                Error = new ApiError
                {
                    Code = code,
                    Message = message,
                    Type = errorType
                }
            };
        }

        public static ApiResponse<T> ValidationError<T>(Dictionary<string, string[]> validationErrors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = ApiResponseConstants.ErrorCodes.ValidationError,
                    Message = ApiResponseConstants.ErrorMessages.ValidationFailed,
                    Type = "Validation",
                    ValidationErrors = validationErrors
                }
            };
        }
    }
}
