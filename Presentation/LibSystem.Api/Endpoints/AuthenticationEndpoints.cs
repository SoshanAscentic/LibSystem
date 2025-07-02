using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using LibSystem.Application.DTOs.Identity;
using LibSystem.Application.Usecases.Identity.LoginUser;
using LibSystem.Application.Usecases.Identity.RegisterUser;
using LibSystem.Application.Usecases.Identity.GetAllUsers;
using MediatR;
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
            [FromBody] LoginRequest request,
            ISender sender)
        {
            var command = new LoginUserCommand(request.Email, request.Password, request.RememberMe);
            var result = await sender.Send(command);
            return result.ToHttpResult("User authenticated successfully");
        }

        private static async Task<IResult> Register(
            [FromBody] RegisterRequest request,
            ISender sender)
        {
            var command = new RegisterUserCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.Role);

            var result = await sender.Send(command);

            if (result.IsSuccess)
            {
                return result.ToCreatedResult($"/api/auth/me", "User registered successfully");
            }

            return result.ToHttpResult();
        }

        private static async Task<IResult> GetCurrentUser(
            ClaimsPrincipal user,
            ISender sender)
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

            var query = new GetUserByIdQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult("User information retrieved successfully");
        }
    }
}