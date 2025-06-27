using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs;
using LibSystem.Application.Usecases.Borrowing.BorrowBook;
using LibSystem.Application.Usecases.Borrowing.GetMemberBorrowingStatus;
using LibSystem.Application.Usecases.Borrowing.ReturnBook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibSystem.Api.Endpoints
{
    public static class BorrowingEndpoints
    {

        public static void MapBorrowingEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/borrowing")
                .WithTags("Borrowing")
                .WithOpenApi();


            // POST /api/borrowing/borrow - Borrow a book
            group.MapPost("/borrow", BorrowBook)
                .WithName("BorrowBook")
                .WithSummary("Borrow a book from the library")
                .WithDescription("Allows a member to borrow an available book, subject to borrowing limits and permissions")
                .Produces<Result<string>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status500InternalServerError);

            // POST /api/borrowing/return - Return a book
            group.MapPost("/return", ReturnBook)
                .WithName("ReturnBook")
                .WithSummary("Return a borrowed book to the library")
                .WithDescription("Allows a member to return a book they have previously borrowed")
                .Produces<Result<string>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status500InternalServerError);


            // GET /api/borrowing/member/{memberId} - Get member borrowing status
            group.MapGet("/member/{memberId:int}", GetMemberBorrowingStatus)
                .WithName("GetMemberBorrowingStatus")
                .WithSummary("Get borrowing status for a specific member")
                .WithDescription("Returns detailed borrowing information including active loans and borrowing history")
                .Produces<Result<BorrowingStatusDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

        }

 
        private static async Task<IResult> BorrowBook(
            [FromBody] BorrowBookCommand command,
            ISender sender)
        {
            // The command validation is handled by FluentValidation in the pipeline
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result)
                : result.Error.Contains("not found")
                    ? Results.NotFound(result)
                    : result.Error.Contains("not available") || result.Error.Contains("limit") || result.Error.Contains("permission")
                        ? Results.Conflict(result)
                        : Results.BadRequest(result);
        }


        private static async Task<IResult> ReturnBook(
            [FromBody] ReturnBookCommand command,
            ISender sender)
        {
            // The command validation is handled by FluentValidation in the pipeline
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result)
                : result.Error.Contains("not found")
                    ? Results.NotFound(result)
                    : result.Error.Contains("not currently borrowed") || result.Error.Contains("already returned")
                        ? Results.Conflict(result)
                        : Results.BadRequest(result);
        }

        private static async Task<IResult> GetMemberBorrowingStatus(int memberId, ISender sender)
        {
            // Basic validation at the endpoint level
            if (memberId <= 0)
            {
                var validationResult = Result<BorrowingStatusDto>.Failure("Member ID must be a positive integer.");
                return Results.BadRequest(validationResult);
            }

            var query = new GetMemberBorrowingStatusQuery(memberId);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result)
                : result.Error.Contains("not found")
                    ? Results.NotFound(result)
                    : Results.Problem(result.Error);
        }

    }
}