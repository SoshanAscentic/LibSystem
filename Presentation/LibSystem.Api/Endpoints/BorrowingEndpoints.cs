using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Borrowing;
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
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // POST /api/borrowing/return - Return a book
            group.MapPost("/return", ReturnBook)
                .WithName("ReturnBook")
                .WithSummary("Return a borrowed book to the library")
                .WithDescription("Allows a member to return a book they have previously borrowed")
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            // GET /api/borrowing/member/{memberId} - Get member borrowing status
            group.MapGet("/member/{memberId:int}", GetMemberBorrowingStatus)
                .WithName("GetMemberBorrowingStatus")
                .WithSummary("Get borrowing status for a specific member")
                .WithDescription("Returns detailed borrowing information including active loans and borrowing history")
                .Produces<ApiResponse<BorrowingStatusDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> BorrowBook(
            [FromBody] BorrowBookCommand command,
            ISender sender)
        {
            var result = await sender.Send(command);

            return result.ToHttpResult("Book borrowed successfully");
        }

        private static async Task<IResult> ReturnBook(
            [FromBody] ReturnBookCommand command,
            ISender sender)
        {
            var result = await sender.Send(command);

            return result.ToHttpResult("Book returned successfully");
        }

        private static async Task<IResult> GetMemberBorrowingStatus(int memberId, ISender sender)
        {
            var query = new GetMemberBorrowingStatusQuery(memberId);
            var result = await sender.Send(query);

            return result.ToHttpResult("Borrowing status retrieved successfully");
        }
    }
}