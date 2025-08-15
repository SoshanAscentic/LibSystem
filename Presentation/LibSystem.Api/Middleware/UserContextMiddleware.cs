using LibSystem.Application.Contracts.Identity;
using System.Security.Claims;

namespace LibSystem.Api.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<UserContextMiddleware> logger;

        public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IMemberSyncService memberSyncService)
        {
            // If user is authenticated, ensure domain member sync
            if (context.User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var memberIdClaim = context.User.FindFirst("MemberId")?.Value;

                    // If user doesn't have member ID in claims, try to get/create domain member
                    if (string.IsNullOrEmpty(memberIdClaim) &&
                        int.TryParse(userIdClaim, out var userId))
                    {
                        var result = await memberSyncService.GetMemberIdForUserAsync(userId);
                        if (result.IsSuccess && result.Value.HasValue)
                        {
                            // Add member ID to current request context
                            var claims = context.User.Claims.ToList();
                            claims.Add(new Claim("MemberId", result.Value.Value.ToString()));
                            var identity = new ClaimsIdentity(claims, context.User.Identity.AuthenticationType);
                            context.User = new ClaimsPrincipal(identity);

                            logger.LogDebug("Added MemberId {MemberId} to user context for user {UserId}",
                                result.Value.Value, userId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error syncing user context for user");
                    // Don't fail the request, just log the warning
                }
            }

            await next(context);
        }
    }

    public static class UserContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserContextMiddleware>();
        }
    }
}