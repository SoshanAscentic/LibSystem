using LibSystem.Application.Common.Models;
using LibSystem.Application.Repositories;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Commands.DeleteBook
{
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Result>
    {
        private readonly IBookRepository bookRepository;
        private readonly IBorrowingRepository borrowingRepository;
        private readonly ILogger<DeleteBookCommandHandler> logger;

        public DeleteBookCommandHandler(
            IBookRepository bookRepository,
            IBorrowingRepository borrowingRepository,
            ILogger<DeleteBookCommandHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.borrowingRepository = borrowingRepository;
            this.logger = logger;
        }

        public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Attempting to delete book with ID: {BookId}", request.BookId);

                var bookId = BookId.Create(request.BookId);
                var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);

                if (book == null)
                {
                    var error = $"Book with ID {request.BookId} was not found.";
                    logger.LogWarning(error);
                    return Result.Failure(error);
                }

                // Business rule: Cannot delete books that are currently borrowed
                var isCurrentlyBorrowed = await borrowingRepository.IsBookCurrentlyBorrowedAsync(bookId, cancellationToken);
                if (isCurrentlyBorrowed)
                {
                    var error = $"Cannot delete book '{book.Title}' as it is currently borrowed.";
                    logger.LogWarning(error);
                    return Result.Failure(error);
                }

                // Delete the book
                bookRepository.Remove(book);
                await bookRepository.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully deleted book: {Title} (ID: {BookId})", book.Title, book.BookId.Value);

                return Result.Success();
            }
            catch (BookNotFoundException ex)
            {
                logger.LogWarning(ex, "Book not found for deletion: {BookId}", request.BookId);
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting book with ID: {BookId}", request.BookId);
                return Result.Failure("An error occurred while deleting the book.");
            }
        }
    }
}
