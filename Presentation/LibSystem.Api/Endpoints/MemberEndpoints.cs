using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Member;
using LibSystem.Application.Usecases.Members.AuthenticateMember;
using LibSystem.Application.Usecases.Members.CreateMember;
using LibSystem.Application.Usecases.Members.GetAllMembers;
using LibSystem.Application.Usecases.Members.GetMembersById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibSystem.Api.Endpoints
{
    public static class MemberEndpoints
    {
        public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/members")
                .WithTags("Members")
                .WithOpenApi();

            // GET /api/members - Get all members (Staff+ only - members shouldn't see other members)
            group.MapGet("/", GetAllMembers)
                .WithName("GetAllMembers")
                .WithSummary("Retrieve all library members")
                .WithDescription("Returns a list of all registered members with their details and permissions (Staff+ only)")
                .Produces<ApiResponse<IReadOnlyList<MemberDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("RequireStaffRole"); // Staff and above only

            // GET /api/members/{id} - Get member by ID (Staff+ can see any member, Members can only see themselves)
            group.MapGet("/{id:int}", GetMemberById)
                .WithName("GetMemberById")
                .WithSummary("Retrieve a specific member by ID")
                .WithDescription("Returns detailed information about a member including borrowing status")
                .Produces<ApiResponse<MemberDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("RequireMemberRole"); // All authenticated users, but will check ownership in handler

            // POST /api/members - Create a new member (Admin only for creating system users)
            group.MapPost("/", CreateMember)
                .WithName("CreateMember")
                .WithSummary("Register a new library member")
                .WithDescription("Creates a new member account with specified member type and permissions (Admin only)")
                .Produces<ApiResponse<MemberDto>>(StatusCodes.Status201Created)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("MemberManagement"); // Admin only

            // POST /api/members/authenticate - Authenticate member (Public - for legacy member login)
            group.MapPost("/authenticate", AuthenticateMember)
                .WithName("AuthenticateMember")
                .WithSummary("Authenticate a library member")
                .WithDescription("Validates member credentials and returns member information (Legacy endpoint)")
                .Produces<ApiResponse<MemberDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .AllowAnonymous(); // Legacy endpoint for member authentication
        }

        private static async Task<IResult> GetAllMembers(ISender sender)
        {
            var query = new GetAllMembersQuery();
            var result = await sender.Send(query);

            return result.ToHttpResult("Members retrieved successfully");
        }

        private static async Task<IResult> GetMemberById(int id, ISender sender)
        {
            var query = new GetMemberByIdQuery(id);
            var result = await sender.Send(query);

            return result.ToHttpResult("Member retrieved successfully");
        }

        private static async Task<IResult> CreateMember(
            [FromBody] CreateMemberCommand command,
            ISender sender)
        {
            var result = await sender.Send(command);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/members/{result.Value.MemberID}", new ApiResponse<MemberDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Member created successfully"
                });
            }

            return result.ToHttpResult();
        }

        private static async Task<IResult> AuthenticateMember(
            [FromBody] AuthenticateMemberCommand command,
            ISender sender)
        {
            var result = await sender.Send(command);

            return result.ToHttpResult("Member authenticated successfully");
        }
    }
}