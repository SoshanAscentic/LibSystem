using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs;
using LibSystem.Application.Members.Queries.GetAllMembers;
using LibSystem.Application.Members.Queries.GetMembersById;
using LibSystem.Application.Usecases.Members.AuthenticateMember;
using LibSystem.Application.Usecases.Members.CreateMember;
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


            // GET /api/members - Get all members
            group.MapGet("/", GetAllMembers)
                .WithName("GetAllMembers")
                .WithSummary("Retrieve all library members")
                .WithDescription("Returns a list of all registered members with their details and permissions")
                .Produces<Result<IReadOnlyList<MemberDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            // GET /api/members/{id} - Get member by ID
            group.MapGet("/{id:int}", GetMemberById)
                .WithName("GetMemberById")
                .WithSummary("Retrieve a specific member by ID")
                .WithDescription("Returns detailed information about a member including borrowing status")
                .Produces<Result<MemberDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);


            // POST /api/members - Create a new member (Sign up)
            group.MapPost("/", CreateMember)
                .WithName("CreateMember")
                .WithSummary("Register a new library member")
                .WithDescription("Creates a new member account with specified member type and permissions")
                .Produces<Result<MemberDto>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            // POST /api/members/authenticate - Authenticate member (Login)
            group.MapPost("/authenticate", AuthenticateMember)
                .WithName("AuthenticateMember")
                .WithSummary("Authenticate a library member")
                .WithDescription("Validates member credentials and returns member information")
                .Produces<Result<MemberDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

        }

        private static async Task<IResult> GetAllMembers(ISender sender)
        {
            // Create query object (no parameters needed for getting all members)
            var query = new GetAllMembersQuery();

            // Send query through MediatR pipeline
            var result = await sender.Send(query);

            // Return appropriate HTTP response based on result
            return result.IsSuccess
                ? Results.Ok(result)           
                : Results.Problem(result.Error); 
        }

        private static async Task<IResult> GetMemberById(int id, ISender sender)
        {
            // Basic validation at the endpoint level (defensive programming)
            if (id <= 0)
            {
                var validationResult = Result<MemberDto>.Failure("Member ID must be a positive integer.");
                return Results.BadRequest(validationResult);
            }

            // Create query with the member ID
            var query = new GetMemberByIdQuery(id);

            // Send through MediatR pipeline
            var result = await sender.Send(query);

            // Return appropriate HTTP response based on result and error content
            return result.IsSuccess
                ? Results.Ok(result)                           
                : result.Error.Contains("not found")          
                    ? Results.NotFound(result)                 
                    : Results.Problem(result.Error);           
        }

        private static async Task<IResult> CreateMember(
            [FromBody] CreateMemberCommand command,  
            ISender sender)
        {
            
            var result = await sender.Send(command);

            // Return appropriate HTTP response
            return result.IsSuccess
                ? Results.Created($"/api/members/{result.Value.MemberID}", result) 
                : Results.BadRequest(result);                                       
        }

        private static async Task<IResult> AuthenticateMember(
            [FromBody] AuthenticateMemberCommand command,
            ISender sender)
        {
            // Send authentication command through MediatR pipeline
            var result = await sender.Send(command);

            // Return appropriate HTTP response based on authentication result
            return result.IsSuccess
                ? Results.Ok(result)                          
                : result.Error.Contains("not found")         
                    ? Results.NotFound(result)                
                    : Results.BadRequest(result);             
        }

    }
}