using LibSystem.Api.Common;
using System.Security.Claims;

namespace LibSystem.Api.Endpoints
{
    public static class DebugEndpoints
    {
        public static void MapDebugEndpoints(this IEndpointRouteBuilder app)
        {
            // Only add debug endpoints in development
            if (app.ServiceProvider.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                var group = app.MapGroup("/api/debug")
                    .WithTags("Debug")
                    .WithOpenApi();

                // GET /api/debug/token - Debug JWT token claims
                group.MapGet("/token", GetTokenInfo)
                    .WithName("GetTokenInfo")
                    .WithSummary("Debug JWT token claims")
                    .WithDescription("Returns information about the current JWT token and claims (Development only)")
                    .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
                    .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                    .RequireAuthorization(); // Requires any valid JWT token

                // GET /api/debug/auth-test - Test different authorization levels
                group.MapGet("/auth-test/public", GetPublicInfo)
                    .WithName("GetPublicInfo")
                    .WithSummary("Test public endpoint")
                    .WithDescription("Public endpoint that doesn't require authentication")
                    .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
                    .AllowAnonymous();

                group.MapGet("/auth-test/member", GetMemberInfo)
                    .WithName("GetMemberInfo")
                    .WithSummary("Test member endpoint")
                    .WithDescription("Endpoint that requires Member role or higher")
                    .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
                    .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                    .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                    .RequireAuthorization("RequireMemberRole");

                group.MapGet("/auth-test/staff", GetStaffInfo)
                    .WithName("GetStaffInfo")
                    .WithSummary("Test staff endpoint")
                    .WithDescription("Endpoint that requires Staff role or higher")
                    .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
                    .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                    .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                    .RequireAuthorization("RequireStaffRole");

                group.MapGet("/auth-test/admin", GetAdminInfo)
                    .WithName("GetAdminInfo")
                    .WithSummary("Test admin endpoint")
                    .WithDescription("Endpoint that requires Administrator role")
                    .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
                    .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                    .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                    .RequireAuthorization("RequireAdministratorRole");
            }
        }

        private static IResult GetTokenInfo(ClaimsPrincipal user)
        {
            var tokenInfo = new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                AuthenticationType = user.Identity?.AuthenticationType,
                Name = user.Identity?.Name,
                Claims = user.Claims.Select(c => new
                {
                    Type = c.Type,
                    Value = c.Value,
                    DisplayName = GetClaimDisplayName(c.Type)
                }).ToList(),
                Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
                MemberId = user.FindFirst("MemberId")?.Value,
                FullName = user.FindFirst("FullName")?.Value,
                Permissions = new
                {
                    CanManageBooks = user.IsInRole("ManagementStaff") || user.IsInRole("Administrator"),
                    CanManageUsers = user.IsInRole("Administrator"),
                    CanBorrowBooks = user.IsInRole("Member") || user.IsInRole("MinorStaff") || user.IsInRole("ManagementStaff") || user.IsInRole("Administrator"),
                    CanViewBooks = user.IsInRole("Member") || user.IsInRole("MinorStaff") || user.IsInRole("ManagementStaff") || user.IsInRole("Administrator"),
                    CanViewMembers = user.IsInRole("MinorStaff") || user.IsInRole("ManagementStaff") || user.IsInRole("Administrator")
                }
            };

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = tokenInfo,
                Message = "Token information retrieved successfully"
            });
        }

        private static IResult GetPublicInfo()
        {
            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { Message = "This is a public endpoint", RequiresAuth = false },
                Message = "Public endpoint accessed successfully"
            });
        }

        private static IResult GetMemberInfo(ClaimsPrincipal user)
        {
            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    Message = "This endpoint requires Member role or higher",
                    UserRole = user.FindFirst(ClaimTypes.Role)?.Value,
                    AllRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                    RequiredPolicy = "RequireMemberRole"
                },
                Message = "Member endpoint accessed successfully"
            });
        }

        private static IResult GetStaffInfo(ClaimsPrincipal user)
        {
            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    Message = "This endpoint requires Staff role or higher",
                    UserRole = user.FindFirst(ClaimTypes.Role)?.Value,
                    AllRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                    RequiredPolicy = "RequireStaffRole"
                },
                Message = "Staff endpoint accessed successfully"
            });
        }

        private static IResult GetAdminInfo(ClaimsPrincipal user)
        {
            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    Message = "This endpoint requires Administrator role",
                    UserRole = user.FindFirst(ClaimTypes.Role)?.Value,
                    AllRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                    RequiredPolicy = "RequireAdministratorRole"
                },
                Message = "Admin endpoint accessed successfully"
            });
        }

        private static string GetClaimDisplayName(string claimType)
        {
            return claimType switch
            {
                ClaimTypes.NameIdentifier => "User ID",
                ClaimTypes.Name => "Username",
                ClaimTypes.Email => "Email",
                ClaimTypes.Role => "Role",
                ClaimTypes.GivenName => "First Name",
                ClaimTypes.Surname => "Last Name",
                "MemberId" => "Member ID",
                "FullName" => "Full Name",
                "IsActive" => "Is Active",
                "jti" => "JWT ID",
                "iat" => "Issued At",
                _ => claimType
            };
        }
    }
}
