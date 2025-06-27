using FluentValidation;
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
                // Continue to the next middleware in the pipeline
                await next(httpContext);
            }
            catch (Exception ex)
            {
                // Log the exception details
                logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);

                // Handle the exception and create appropriate response
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Set response content type
            context.Response.ContentType = "application/json";

            // Create error response based on exception type
            var response = exception switch
            {
                // Validation exceptions from FluentValidation
                ValidationException validationEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred.",
                    Errors = validationEx.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                },

                // Custom application validation exceptions
                LibSystem.Application.Common.Exceptions.ValidationException appValidationEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred.",
                    Errors = appValidationEx.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) 
                },

                // Domain exceptions - business rule violations
                DomainException domainEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Title = "Business Rule Violation",
                    Detail = domainEx.Message
                },

                // Entity not found exceptions
                LibSystem.Application.Common.Exceptions.NotFoundException notFoundEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message
                },

                // Argument exceptions - invalid input
                ArgumentException argEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Argument",
                    Detail = argEx.Message
                },

                // Unauthorized access exceptions
                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Title = "Unauthorized",
                    Detail = "You are not authorized to access this resource."
                },

                // Database operation exceptions
                InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("database") => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Title = "Database Operation Error",
                    Detail = "A database operation error occurred. Please try again."
                },

                // Generic operation exceptions
                InvalidOperationException invalidOpEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Operation",
                    Detail = invalidOpEx.Message
                },

                // All other exceptions - internal server error
                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An internal server error occurred. Please try again later."
                }
            };

            // Set HTTP status code
            context.Response.StatusCode = response.StatusCode;

            // Serialize and write the error response
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
    public class ErrorResponse
    {

        public int StatusCode { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

 
        public Dictionary<string, string[]>? Errors { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        public string TraceId { get; set; } = Guid.NewGuid().ToString();
    }

    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}