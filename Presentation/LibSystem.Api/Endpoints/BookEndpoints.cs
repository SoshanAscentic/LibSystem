using LibSystem.Application.Common.Models;
using LibSystem.Application.Usecases.Books.CreateBook;
using LibSystem.Application.Usecases.Books.DeleteBook;
using LibSystem.Application.Usecases.Books.GetAllBooks;
using LibSystem.Application.Usecases.Books.GetBookById;
using LibSystem.Application.Usecases.Books.GetBooksByAuthor;
using LibSystem.Application.Usecases.Books.GetBooksByCategory;
using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using LibSystem.Application.DTOs.Book;

namespace LibSystem.Api.Endpoints
{
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/books")
                .WithTags("Books")
                .WithOpenApi();

            // GET /api/books - Get all books (All authenticated users can view books)
            group.MapGet("/", GetAllBooks)
                .WithName("GetAllBooks")
                .WithSummary("Retrieve all books in the library")
                .WithDescription("Returns a list of all books with their availability status and details")
                .Produces<ApiResponse<IReadOnlyList<BookDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("RequireMemberRole"); // All authenticated users can view books

            // GET /api/books/{id} - Get book by ID (All authenticated users can view books)
            group.MapGet("/{id:int}", GetBookById)
                .WithName("GetBookById")
                .WithSummary("Retrieve a specific book by ID")
                .WithDescription("Returns detailed information about a book including availability status")
                .Produces<ApiResponse<BookDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("RequireMemberRole");

            // GET /api/books/category/{category} - Get books by category (All authenticated users)
            group.MapGet("/category/{category}", GetBooksByCategory)
                .WithName("GetBooksByCategory")
                .WithSummary("Retrieve books by category")
                .WithDescription("Returns books filtered by category (Fiction, History, Child)")
                .Produces<ApiResponse<IReadOnlyList<BookDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("RequireMemberRole");

            // GET /api/books/author/{author} - Get books by author (All authenticated users)
            group.MapGet("/author/{author}", GetBooksByAuthor)
                .WithName("GetBooksByAuthor")
                .WithSummary("Retrieve books by author")
                .WithDescription("Returns books filtered by author name (supports partial matching)")
                .Produces<ApiResponse<IReadOnlyList<BookDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("RequireMemberRole");

            // POST /api/books - Create a new book (Management Staff + Admin only)
            group.MapPost("/", CreateBook)
                .WithName("CreateBook")
                .WithSummary("Add a new book to the library")
                .WithDescription("Creates a new book with validation for duplicate titles and years (Management Staff+ only)")
                .Produces<ApiResponse<BookDto>>(StatusCodes.Status201Created)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("BookManagement"); // Management Staff and Admin only

            // DELETE /api/books/{id} - Delete a book (Management Staff + Admin only)
            group.MapDelete("/{id:int}", DeleteBook)
                .WithName("DeleteBook")
                .WithSummary("Remove a book from the library")
                .WithDescription("Deletes a book if it's not currently borrowed (Management Staff+ only)")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse>(StatusCodes.Status409Conflict)
                .Produces<ApiResponse>(StatusCodes.Status500InternalServerError)
                .RequireAuthorization("BookManagement"); // Management Staff and Admin only
        }

        private static async Task<IResult> GetAllBooks(ISender sender)
        {
            var query = new GetAllBooksQuery();
            var result = await sender.Send(query);

            return result.ToHttpResult("Books retrieved successfully");
        }

        private static async Task<IResult> GetBookById(int id, ISender sender)
        {
            var query = new GetBookByIdQuery(id);
            var result = await sender.Send(query);

            return result.ToHttpResult("Book retrieved successfully");
        }

        private static async Task<IResult> GetBooksByCategory(string category, ISender sender)
        {
            var query = new GetBooksByCategoryQuery(category);
            var result = await sender.Send(query);

            return result.ToHttpResult($"Books in category '{category}' retrieved successfully");
        }

        private static async Task<IResult> GetBooksByAuthor(string author, ISender sender)
        {
            var query = new GetBooksByAuthorQuery(author);
            var result = await sender.Send(query);

            return result.ToHttpResult($"Books by author '{author}' retrieved successfully");
        }

        private static async Task<IResult> CreateBook(
            [FromBody] CreateBookCommand command,
            ISender sender)
        {
            var result = await sender.Send(command);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/books/{result.Value.BookId}", new ApiResponse<BookDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Book created successfully"
                });
            }

            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteBook(int id, ISender sender)
        {
            var command = new DeleteBookCommand(id);
            var result = await sender.Send(command);

            return result.ToNoContentResult();
        }
    }
}