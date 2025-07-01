using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using LibSystem.Identity.Contracts;
using LibSystem.Identity.DTOs;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibSystem.Api.Endpoints
{
    public static class AuthenticationEndpoints
    {
        public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Authentication")
                .WithOpenApi();

            // POST /api/auth/login - User login
            group.MapPost("/login", Login)
                .WithName("Login")
                .WithSummary("Authenticate user and get JWT token")
                .WithDescription("Validates user credentials and returns JWT token with user information")
                .Produces<ApiResponse<AuthenticationResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .AllowAnonymous();

            // POST /api/auth/register - User registration
            group.MapPost("/register", Register)
                .WithName("Register")
                .WithSummary("Register a new user account")
                .WithDescription("Creates a new user account with specified role and returns JWT token")
                .Produces<ApiResponse<AuthenticationResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .AllowAnonymous();

            // POST /api/auth/logout - User logout
            group.MapPost("/logout", Logout)
                .WithName("Logout")
                .WithSummary("Logout current user")
                .WithDescription("Logs out the currently authenticated user")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            // POST /api/auth/refresh - Refresh JWT token
            group.MapPost("/refresh", RefreshToken)
                .WithName("RefreshToken")
                .WithSummary("Refresh JWT token")
                .WithDescription("Generates a new JWT token using the existing token")
                .Produces<ApiResponse<AuthenticationResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .AllowAnonymous();

            // POST /api/auth/change-password - Change password
            group.MapPost("/change-password", ChangePassword)
                .WithName("ChangePassword")
                .WithSummary("Change user password")
                .WithDescription("Changes the password for the currently authenticated user")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            // GET /api/auth/me - Get current user info
            group.MapGet("/me", GetCurrentUser)
                .WithName("GetCurrentUser")
                .WithSummary("Get current user information")
                .WithDescription("Returns information about the currently authenticated user")
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();
        }

        private static async Task<IResult> Login(
            [FromBody] Identity.DTOs.LoginRequest request,
            Identity.Contracts.IAuthenticationService authService)
        {
            var result = await authService.LoginAsync(request);
            return result.ToHttpResult("User authenticated successfully");
        }

        private static async Task<IResult> Register(
            [FromBody] Identity.DTOs.RegisterRequest request,
            Identity.Contracts.IAuthenticationService authService)
        {
            var result = await authService.RegisterAsync(request);

            if (result.IsSuccess)
            {
                return result.ToCreatedResult($"/api/auth/me", "User registered successfully");
            }

            return result.ToHttpResult();
        }

        private static async Task<IResult> Logout(
            ClaimsPrincipal user,
            Identity.Contracts.IAuthenticationService authService)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Results.Problem(
                    detail: "Invalid user token",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );
            }

            var result = await authService.LogoutAsync(userId);
            return result.ToHttpResult("Successfully logged out");
        }

        private static async Task<IResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            Identity.Contracts.IAuthenticationService authService)
        {
            var result = await authService.RefreshTokenAsync(request);
            return result.ToHttpResult("Token refreshed successfully");
        }

        private static async Task<IResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            ClaimsPrincipal user,
            Identity.Contracts.IAuthenticationService authService)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Results.Problem(
                    detail: "Invalid user token",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );
            }

            var result = await authService.ChangePasswordAsync(userId, request);
            return result.ToHttpResult("Password changed successfully");
        }

        private static async Task<IResult> GetCurrentUser(
            ClaimsPrincipal user,
            IUserManagementService userService)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Results.Problem(
                    detail: "Invalid user token",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );
            }

            var result = await userService.GetUserByIdAsync(userId);
            return result.ToHttpResult("User information retrieved successfully");
        }
    }
}