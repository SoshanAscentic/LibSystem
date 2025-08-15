using LibSystem.Api.Common;

namespace LibSystem.Api.Extensions
{
    public static class EndpointExtensions
    {
        public static RouteGroupBuilder WithStandardConfiguration(this RouteGroupBuilder group, string tag)
        {
            return group
                .WithTags(tag)
                .WithOpenApi()
                .AddEndpointFilter<ValidationEndpointFilter>()
                .AddEndpointFilter<LoggingEndpointFilter>();
        }
        public static RouteHandlerBuilder WithStandardResponses<T>(this RouteHandlerBuilder builder)
        {
            return builder
                .Produces<ApiResponse<T>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        }

        public static RouteHandlerBuilder WithConflictResponses<T>(this RouteHandlerBuilder builder)
        {
            return builder
                .WithStandardResponses<T>()
                .Produces<ApiResponse>(StatusCodes.Status409Conflict);
        }
        public static RouteHandlerBuilder WithAuthorizationResponses<T>(this RouteHandlerBuilder builder)
        {
            return builder
                .WithConflictResponses<T>()
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden);
        }

        public class LoggingEndpointFilter : IEndpointFilter
        {
            private readonly ILogger<LoggingEndpointFilter> logger;

            public LoggingEndpointFilter(ILogger<LoggingEndpointFilter> logger)
            {
                this.logger = logger;
            }

            public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
            {
                var httpContext = context.HttpContext;
                var requestPath = httpContext.Request.Path;
                var requestMethod = httpContext.Request.Method;

                logger.LogInformation("Processing request: {Method} {Path}", requestMethod, requestPath);

                var startTime = DateTime.UtcNow;
                var result = await next(context);
                var duration = DateTime.UtcNow - startTime;

                logger.LogInformation("Completed request: {Method} {Path} in {Duration}ms",
                    requestMethod, requestPath, duration.TotalMilliseconds);

                return result;
            }
        }

        public class ValidationEndpointFilter : IEndpointFilter
        {
            public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
            {
                // Add any cross-cutting validation logic here
                return await next(context);
            }
        }
    }
}
