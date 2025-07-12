using LibSystem.Api.Common;
using LibSystem.Application.Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibSystem.Api.Endpoints
{
    public static class PermissionVerificationEndpoints
    {
        public static void MapPermissionVerificationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Permission Verification")
                .RequireAuthorization() // All endpoints require authentication
                .WithOpenApi();

            // GET /api/auth/permissions - Get user permissions
            group.MapGet("/permissions", GetUserPermissions)
                .WithName("GetUserPermissions")
                .WithSummary("Get current user's permissions");

            // GET /api/auth/verify-access - Verify specific resource access
            group.MapGet("/verify-access", VerifyResourceAccess)
                .WithName("VerifyResourceAccess")
                .WithSummary("Verify access to specific resource");
        }

        private static async Task<IResult> GetUserPermissions(
            ClaimsPrincipal user,
            ICurrentUserService currentUserService)
        {
            try
            {
                if (!currentUserService.IsAuthenticated)
                {
                    return Results.Unauthorized();
                }

                // Get permissions based on CURRENT database roles (not token claims)
                var permissions = new
                {
                    canEdit = currentUserService.CanManageBooks,
                    canDelete = currentUserService.CanManageBooks,
                    canAdd = currentUserService.CanManageBooks,
                    canBorrow = currentUserService.CanBorrowBooks,
                    canViewBorrowing = IsStaffOrAbove(currentUserService),
                    canManageUsers = currentUserService.CanManageUsers,
                    canReturnBooks = currentUserService.CanBorrowBooks,
                    canViewAllBorrowings = IsStaffOrAbove(currentUserService),
                    userRole = GetPrimaryRole(currentUserService.Roles),
                    userId = currentUserService.UserId,
                    memberId = currentUserService.MemberId
                };

                return Results.Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = permissions,
                    Message = "Permissions retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: "Failed to retrieve permissions",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        private static async Task<IResult> VerifyResourceAccess(
            [FromQuery] string resource,
            [FromQuery] string action,
            [FromQuery] int? resourceId,
            ClaimsPrincipal user,
            ICurrentUserService currentUserService)
        {
            try
            {
                if (!currentUserService.IsAuthenticated)
                {
                    return Results.Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Data = new { hasAccess = false },
                        Message = "User not authenticated"
                    });
                }

                var hasAccess = await VerifyAccess(resource, action, resourceId, currentUserService);

                return Results.Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { hasAccess },
                    Message = "Access verification completed"
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { hasAccess = false },
                    Message = "Access denied due to verification error"
                });
            }
        }

        private static async Task<bool> VerifyAccess(
            string resource,
            string action,
            int? resourceId,
            ICurrentUserService currentUserService)
        {
            // Verify based on resource and action
            return (resource.ToLower(), action.ToLower()) switch
            {
                // Books access
                ("books", "read") => true, // Anyone can read books
                ("books", "create") => currentUserService.CanManageBooks,
                ("books", "update") => currentUserService.CanManageBooks,
                ("books", "delete") => currentUserService.CanManageBooks,

                // Members access
                ("members", "read") => VerifyMemberAccess("read", resourceId, currentUserService),
                ("members", "update") => VerifyMemberAccess("update", resourceId, currentUserService),
                ("members", "create") => currentUserService.CanManageUsers,
                ("members", "delete") => currentUserService.CanManageUsers,

                // Borrowing access
                ("borrowing", "read") => VerifyBorrowingAccess("read", resourceId, currentUserService),
                ("borrowing", "create") => currentUserService.CanBorrowBooks,
                ("borrowing", "manage") => IsStaffOrAbove(currentUserService),

                // User management
                ("users", "read") => currentUserService.CanManageUsers,
                ("users", "update") => currentUserService.CanManageUsers,
                ("users", "create") => currentUserService.CanManageUsers,
                ("users", "delete") => currentUserService.CanManageUsers,

                _ => false // Deny by default
            };
        }

        private static bool VerifyMemberAccess(string action, int? memberId, ICurrentUserService currentUserService)
        {
            return action switch
            {
                "read" => IsStaffOrAbove(currentUserService) ||
                         (memberId.HasValue && currentUserService.MemberId == memberId.Value),
                "update" => currentUserService.CanManageUsers ||
                           (memberId.HasValue && currentUserService.MemberId == memberId.Value),
                _ => false
            };
        }

        private static bool VerifyBorrowingAccess(string action, int? memberId, ICurrentUserService currentUserService)
        {
            return action switch
            {
                "read" => IsStaffOrAbove(currentUserService) ||
                         (memberId.HasValue && currentUserService.MemberId == memberId.Value),
                _ => false
            };
        }

        private static bool IsStaffOrAbove(ICurrentUserService currentUserService)
        {
            return currentUserService.IsInRole("MinorStaff") ||
                   currentUserService.IsInRole("ManagementStaff") ||
                   currentUserService.IsInRole("Administrator");
        }

        private static string GetPrimaryRole(IList<string> roles)
        {
            if (roles.Contains("Administrator")) return "Administrator";
            if (roles.Contains("ManagementStaff")) return "ManagementStaff";
            if (roles.Contains("MinorStaff")) return "MinorStaff";
            if (roles.Contains("Member")) return "Member";
            return "Unknown";
        }
    }
}