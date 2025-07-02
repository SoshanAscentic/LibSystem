// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteBookCommandHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.DeleteBook
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.Contracts.UoW;
    using LibSystem.Domain.Exceptions;
    using LibSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

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
                if (request.bookId <= 0)
                {
                    return Result.Failure(DomainErrors.General.InvalidId("Book"));
                }

                this.logger.LogInformation("Attempting to delete book with ID: {BookId}", request.bookId);

                var bookId = BookId.Create(request.bookId);
                var book = await this.bookRepository.GetByIdAsync(bookId, cancellationToken);

                if (book == null)
                {
                    this.logger.LogWarning("Book not found for deletion: {BookId}", request.bookId);
                    return Result.Failure(DomainErrors.Book.NotFound(request.bookId));
                }

                // Business rule: Cannot delete books that are currently borrowed
                var isCurrentlyBorrowed = await this.borrowingRepository.IsBookCurrentlyBorrowedAsync(bookId, cancellationToken);
                if (isCurrentlyBorrowed)
                {
                    this.logger.LogWarning("Cannot delete currently borrowed book: {Title} (ID: {BookId})", book.Title, request.bookId);
                    return Result.Failure(DomainErrors.Book.CurrentlyBorrowed(book.Title));
                }

                // Delete the book (stages the change)
                this.bookRepository.Remove(book);

                // Save through UnitOfWork
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully deleted book: {Title} (ID: {BookId})", book.Title, book.BookId.Value);

                return Result.Success();
            }
            catch (BookNotFoundException ex)
            {
                this.logger.LogWarning(ex, "Book not found for deletion: {BookId}", request.bookId);
                return Result.Failure(DomainErrors.Book.NotFound(request.bookId));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID") || ex.Message.Contains("positive"))
            {
                this.logger.LogWarning(ex, "Invalid book ID provided: {BookId}", request.bookId);
                return Result.Failure(DomainErrors.General.InvalidId("Book"));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error deleting book with ID: {BookId}", request.bookId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
