using FluentValidation;
using LibSystem.Api.Common;
using LibSystem.Application.Common.Enums;
using LibSystem.Application.Common.Models;
using LibSystem.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace LibSystem.Api.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path} | TraceId: {TraceId}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.TraceIdentifier);

                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                // FluentValidation exceptions
                ValidationException validationEx => CreateValidationErrorResponse(validationEx, context.TraceIdentifier),

                // Custom application validation exceptions
                LibSystem.Application.Common.Exceptions.ValidationException appValidationEx =>
                    CreateApplicationValidationErrorResponse(appValidationEx, context.TraceIdentifier),

                // Domain exceptions - business rule violations
                DomainException domainEx => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "Domain.BusinessRuleViolation",
                        Message = domainEx.Message,
                        Type = ErrorType.Failure.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Entity not found exceptions
                LibSystem.Application.Common.Exceptions.NotFoundException notFoundEx => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.NotFound",
                        Message = notFoundEx.Message,
                        Type = ErrorType.NotFound.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Argument exceptions - invalid input
                ArgumentException argEx => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.InvalidArgument",
                        Message = argEx.Message,
                        Type = ErrorType.Validation.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Unauthorized access exceptions
                UnauthorizedAccessException => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.Unauthorized",
                        Message = "You are not authorized to access this resource.",
                        Type = ErrorType.Unauthorized.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Database operation exceptions
                InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("database") => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.DatabaseError",
                        Message = "A database operation error occurred. Please try again.",
                        Type = ErrorType.Failure.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Generic operation exceptions
                InvalidOperationException invalidOpEx => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.InvalidOperation",
                        Message = invalidOpEx.Message,
                        Type = ErrorType.Failure.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Timeout exceptions
                TimeoutException timeoutEx => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.Timeout",
                        Message = "The operation timed out. Please try again.",
                        Type = ErrorType.Failure.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // Task cancellation exceptions
                TaskCanceledException => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.RequestCanceled",
                        Message = "The request was canceled.",
                        Type = ErrorType.Failure.ToString()
                    },
                    TraceId = context.TraceIdentifier
                },

                // All other exceptions - internal server error
                _ => new ApiResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "General.InternalError",
                        Message = "An internal server error occurred. Please try again later.",
                        Type = ErrorType.Failure.ToString(),
                        Details = CreateErrorDetails(exception, context)
                    },
                    TraceId = context.TraceIdentifier
                }
            };

            // Set appropriate HTTP status code based on error type
            context.Response.StatusCode = GetStatusCodeForErrorType(response.Error?.Type);

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static ApiResponse CreateValidationErrorResponse(ValidationException validationEx, string traceId)
        {
            var validationErrors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return new ApiResponse
            {
                Success = false,
                Error = new ApiError
                {
                    Code = "General.ValidationFailed",
                    Message = "One or more validation errors occurred.",
                    Type = ErrorType.Validation.ToString(),
                    ValidationErrors = validationErrors
                },
                TraceId = traceId
            };
        }

        private static ApiResponse CreateApplicationValidationErrorResponse(
            LibSystem.Application.Common.Exceptions.ValidationException appValidationEx,
            string traceId)
        {
            return new ApiResponse
            {
                Success = false,
                Error = new ApiError
                {
                    Code = "General.ValidationFailed",
                    Message = "One or more validation errors occurred.",
                    Type = ErrorType.Validation.ToString(),
                    ValidationErrors = appValidationEx.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                },
                TraceId = traceId
            };
        }

        private static Dictionary<string, object> CreateErrorDetails(Exception exception, HttpContext context)
        {
            var details = new Dictionary<string, object>
            {
                ["exceptionType"] = exception.GetType().Name,
                ["requestPath"] = context.Request.Path.ToString(),
                ["requestMethod"] = context.Request.Method,
                ["timestamp"] = DateTime.UtcNow.ToString("O")
            };

            // Add inner exception details if available
            if (exception.InnerException != null)
            {
                details["innerException"] = new
                {
                    Type = exception.InnerException.GetType().Name,
                    Message = exception.InnerException.Message
                };
            }

            return details;
        }

        private static int GetStatusCodeForErrorType(string? errorType)
        {
            return errorType switch
            {
                nameof(ErrorType.Validation) => (int)HttpStatusCode.BadRequest,
                nameof(ErrorType.NotFound) => (int)HttpStatusCode.NotFound,
                nameof(ErrorType.Conflict) => (int)HttpStatusCode.Conflict,
                nameof(ErrorType.Unauthorized) => (int)HttpStatusCode.Unauthorized,
                nameof(ErrorType.Forbidden) => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }
    }
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}