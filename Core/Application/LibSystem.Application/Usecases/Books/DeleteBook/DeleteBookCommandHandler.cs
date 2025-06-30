using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.Contracts.UoW;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.DeleteBook
{
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Result>
    {
        private readonly IBookRepository bookRepository;
        private readonly IBorrowingRepository borrowingRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<DeleteBookCommandHandler> logger;

        public DeleteBookCommandHandler(
            IBookRepository bookRepository,
            IBorrowingRepository borrowingRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteBookCommandHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.borrowingRepository = borrowingRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.BookId <= 0)
                {
                    return Result.Failure(DomainErrors.General.InvalidId("Book"));
                }

                logger.LogInformation("Attempting to delete book with ID: {BookId}", request.BookId);

                var bookId = BookId.Create(request.BookId);
                var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);

                if (book == null)
                {
                    logger.LogWarning("Book not found for deletion: {BookId}", request.BookId);
                    return Result.Failure(DomainErrors.Book.NotFound(request.BookId));
                }

                // Business rule: Cannot delete books that are currently borrowed
                var isCurrentlyBorrowed = await borrowingRepository.IsBookCurrentlyBorrowedAsync(bookId, cancellationToken);
                if (isCurrentlyBorrowed)
                {
                    logger.LogWarning("Cannot delete currently borrowed book: {Title} (ID: {BookId})", book.Title, request.BookId);
                    return Result.Failure(DomainErrors.Book.CurrentlyBorrowed(book.Title));
                }

                // Delete the book (stages the change)
                bookRepository.Remove(book);

                // Save through UnitOfWork
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully deleted book: {Title} (ID: {BookId})", book.Title, book.BookId.Value);

                return Result.Success();
            }
            catch (BookNotFoundException ex)
            {
                logger.LogWarning(ex, "Book not found for deletion: {BookId}", request.BookId);
                return Result.Failure(DomainErrors.Book.NotFound(request.BookId));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID") || ex.Message.Contains("positive"))
            {
                logger.LogWarning(ex, "Invalid book ID provided: {BookId}", request.BookId);
                return Result.Failure(DomainErrors.General.InvalidId("Book"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error deleting book with ID: {BookId}", request.BookId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
