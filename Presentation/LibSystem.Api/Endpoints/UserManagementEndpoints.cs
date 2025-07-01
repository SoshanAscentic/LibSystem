using LibSystem.Api.Common;
using LibSystem.Identity.Contracts;
using LibSystem.Identity.DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using LibSystem.Api.Extensions;


namespace LibSystem.Infrastructure.Identity.Endpoints
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

            // PUT /api/users/{id} - Update user
            group.MapPut("/{id:int}", UpdateUser)
                .WithName("UpdateUser")
                .WithSummary("Update user information")
                .WithDescription("Updates user details (Admin only)")
                .Produces<ApiResponse<UserDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // POST /api/users/{id}/deactivate - Deactivate user
            group.MapPost("/{id:int}/deactivate", DeactivateUser)
                .WithName("DeactivateUser")
                .WithSummary("Deactivate user account")
                .WithDescription("Deactivates a user account (Admin only)")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // POST /api/users/{id}/activate - Activate user
            group.MapPost("/{id}/activate", ActivateUser)
                .WithName("ActivateUser")
                .WithSummary("Activate user account")
                .WithDescription("Activates a user account (Admin only)")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // POST /api/users/assign-role - Assign role to user
            group.MapPost("/assign-role", AssignRole)
                .WithName("AssignRole")
                .WithSummary("Assign role to user")
                .WithDescription("Assigns a role to a user (Admin only)")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // DELETE /api/users/{id}/roles/{role} - Remove role from user
            group.MapDelete("/{id:int}/roles/{role}", RemoveRole)
                .WithName("RemoveRole")
                .WithSummary("Remove role from user")
                .WithDescription("Removes a role from a user (Admin only)")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // GET /api/users/{id}/roles - Get user roles
            group.MapGet("/{id:int}/roles", GetUserRoles)
                .WithName("GetUserRoles")
                .WithSummary("Get user roles")
                .WithDescription("Returns all roles assigned to a user (Admin only)")
                .Produces<ApiResponse<IReadOnlyList<string>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> GetAllUsers(IUserManagementService userService)
        {
            var result = await userService.GetAllUsersAsync();
            return result.ToHttpResult("Users retrieved successfully");
        }

        private static async Task<IResult> GetUserById(int id, IUserManagementService userService)
        {
            var result = await userService.GetUserByIdAsync(id);
            return result.ToHttpResult("User retrieved successfully");
        }

        private static async Task<IResult> UpdateUser(
            int id,
            [FromBody] UpdateUserRequest request,
            IUserManagementService userService)
        {
            var result = await userService.UpdateUserAsync(id, request);
            return result.ToHttpResult("User updated successfully");
        }

        private static async Task<IResult> DeactivateUser(int id, IUserManagementService userService)
        {
            var result = await userService.DeactivateUserAsync(id);
            return result.ToHttpResult("User deactivated successfully");
        }

        private static async Task<IResult> ActivateUser(int id, IUserManagementService userService)
        {
            var result = await userService.ActivateUserAsync(id);
            return result.ToHttpResult("User activated successfully");
        }

        private static async Task<IResult> AssignRole(
            [FromBody] AssignRoleRequest request,
            IUserManagementService userService)
        {
            var result = await userService.AssignRoleAsync(request);
            return result.ToHttpResult($"Role '{request.Role}' assigned successfully");
        }

        private static async Task<IResult> RemoveRole(
            int id,
            string role,
            IUserManagementService userService)
        {
            var result = await userService.RemoveRoleAsync(id, role);
            return result.ToHttpResult($"Role '{role}' removed successfully");
        }

        private static async Task<IResult> GetUserRoles(int id, IUserManagementService userService)
        {
            var result = await userService.GetUserRolesAsync(id);
            return result.ToHttpResult("User roles retrieved successfully");
        }
    }
}
