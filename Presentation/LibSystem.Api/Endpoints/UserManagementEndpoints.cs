using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using LibSystem.Application.DTOs.Identity;
using LibSystem.Application.Usecases.Identity.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibSystem.Api.Endpoints
{
    public static class UserManagementEndpoints
    {
        public static void MapUserManagementEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users")
                .WithTags("User Management")
                .WithOpenApi()
                .RequireAuthorization("RequireAdministratorRole"); // Only administrators can manage users

            // GET /api/users - Get all users
            group.MapGet("/", GetAllUsers)
                .WithName("GetAllUsers")
                .WithSummary("Retrieve all users")
                .WithDescription("Returns a list of all users in the system (Admin only)")
                .Produces<ApiResponse<IReadOnlyList<UserDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // GET /api/users/{id} - Get user by ID
            group.MapGet("/{id:int}", GetUserById)
                .WithName("GetUserById")
                .WithSummary("Retrieve user by ID")
                .WithDescription("Returns detailed information about a specific user (Admin only)")
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> GetAllUsers(ISender sender)
        {
            var query = new GetAllUsersQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult("Users retrieved successfully");
        }

        private static async Task<IResult> GetUserById(int id, ISender sender)
        {
            var query = new GetUserByIdQuery(id);
            var result = await sender.Send(query);
            return result.ToHttpResult("User retrieved successfully");
        }
    }
}