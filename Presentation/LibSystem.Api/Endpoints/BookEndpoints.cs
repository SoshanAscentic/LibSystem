using LibSystem.Application.Books.Commands.DeleteBook;
using LibSystem.Application.Books.Queries.GetAllBooks;
using LibSystem.Application.Books.Queries.GetBookById;
using LibSystem.Application.Books.Queries.GetBooksByAuthor;
using LibSystem.Application.Books.Queries.GetBooksByCategory;
using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs;
using LibSystem.Application.Usecases.Books.CreateBook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibSystem.Api.Endpoints
{
    public static class BookEndpoints
    {

        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/books")
                .WithTags("Books")
                .WithOpenApi();


            // GET /api/books - Get all books
            group.MapGet("/", GetAllBooks)
                .WithName("GetAllBooks")
                .WithSummary("Retrieve all books in the library")
                .WithDescription("Returns a list of all books with their availability status and details")
                .Produces<Result<IReadOnlyList<BookDto>>>(StatusCodes.Status200OK)
                .Produces<Result>(StatusCodes.Status500InternalServerError);

            // GET /api/books/{id} - Get book by ID
            group.MapGet("/{id:int}", GetBookById)
                .WithName("GetBookById")
                .WithSummary("Retrieve a specific book by ID")
                .WithDescription("Returns detailed information about a book including availability status")
                .Produces<Result<BookDto>>(StatusCodes.Status200OK)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status500InternalServerError);

            // GET /api/books/category/{category} - Get books by category
            group.MapGet("/category/{category}", GetBooksByCategory)
                .WithName("GetBooksByCategory")
                .WithSummary("Retrieve books by category")
                .WithDescription("Returns books filtered by category (Fiction, History, Child)")
                .Produces<Result<IReadOnlyList<BookDto>>>(StatusCodes.Status200OK)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status500InternalServerError);

            // GET /api/books/author/{author} - Get books by author
            group.MapGet("/author/{author}", GetBooksByAuthor)
                .WithName("GetBooksByAuthor")
                .WithSummary("Retrieve books by author")
                .WithDescription("Returns books filtered by author name (supports partial matching)")
                .Produces<Result<IReadOnlyList<BookDto>>>(StatusCodes.Status200OK)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status500InternalServerError);


            // POST /api/books - Create a new book
            group.MapPost("/", CreateBook)
                .WithName("CreateBook")
                .WithSummary("Add a new book to the library")
                .WithDescription("Creates a new book with validation for duplicate titles and years")
                .Produces<Result<BookDto>>(StatusCodes.Status201Created)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status409Conflict)
                .Produces<Result>(StatusCodes.Status500InternalServerError);

            // DELETE /api/books/{id} - Delete a book
            group.MapDelete("/{id:int}", DeleteBook)
                .WithName("DeleteBook")
                .WithSummary("Remove a book from the library")
                .WithDescription("Deletes a book if it's not currently borrowed")
                .Produces<Result>(StatusCodes.Status204NoContent)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status409Conflict)
                .Produces<Result>(StatusCodes.Status500InternalServerError);
        }


        private static async Task<IResult> GetAllBooks(ISender sender)
        {
            var query = new GetAllBooksQuery();
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.Problem(result.Error);
        }

    
        private static async Task<IResult> GetBookById(int id, ISender sender)
        {
            // Basic validation at the endpoint level
            if (id <= 0)
            {
                var validationResult = Result<BookDto>.Failure("Book ID must be a positive integer.");
                return Results.BadRequest(validationResult);
            }

            var query = new GetBookByIdQuery(id);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result)
                : result.Error.Contains("not found")
                    ? Results.NotFound(result)
                    : Results.Problem(result.Error);
        }

  
        private static async Task<IResult> GetBooksByCategory(string category, ISender sender)
        {
            // Basic validation at the endpoint level
            if (string.IsNullOrWhiteSpace(category))
            {
                var validationResult = Result<IReadOnlyList<BookDto>>.Failure("Category cannot be empty.");
                return Results.BadRequest(validationResult);
            }

            var query = new GetBooksByCategoryQuery(category);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }

   
        private static async Task<IResult> GetBooksByAuthor(string author, ISender sender)
        {
            // Basic validation at the endpoint level
            if (string.IsNullOrWhiteSpace(author))
            {
                var validationResult = Result<IReadOnlyList<BookDto>>.Failure("Author name cannot be empty.");
                return Results.BadRequest(validationResult);
            }

            var query = new GetBooksByAuthorQuery(author);
            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }

  
        private static async Task<IResult> CreateBook(
            [FromBody] CreateBookCommand command,
            ISender sender)
        {
            // The command validation is handled by FluentValidation in the pipeline
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/books/{result.Value.BookId}", result)
                : result.Error.Contains("already exists")
                    ? Results.Conflict(result)
                    : Results.BadRequest(result);
        }

  
        private static async Task<IResult> DeleteBook(int id, ISender sender)
        {
            // Basic validation at the endpoint level
            if (id <= 0)
            {
                var validationResult = Result.Failure("Book ID must be a positive integer.");
                return Results.BadRequest(validationResult);
            }

            var command = new DeleteBookCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : result.Error.Contains("not found")
                    ? Results.NotFound(result)
                    : result.Error.Contains("currently borrowed")
                        ? Results.Conflict(result)
                        : Results.Problem(result.Error);
        }
    }
}