using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.Contracts.UoW;
using LibSystem.Application.DTOs.Book;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.CreateBook
{
    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Result<BookDto>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<CreateBookCommandHandler> logger;

        public CreateBookCommandHandler(
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateBookCommandHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<BookDto>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Creating book: {Title} by {Author}", request.Title, request.Author);

                // Check for duplicates (business rule: unique title + year combination)
                var existingBook = await bookRepository.GetByTitleAndYearAsync(
                    request.Title, request.PublicationYear, cancellationToken);

                if (existingBook != null)
                {
                    logger.LogWarning("Duplicate book creation attempted: {Title} ({Year})", request.Title, request.PublicationYear);
                    return Result<BookDto>.Failure(DomainErrors.Book.AlreadyExists(request.Title, request.PublicationYear));
                }

                // Create new book using domain factory method
                var book = Book.Create(
                    request.Title,
                    request.Author,
                    request.PublicationYear,
                    (Book.BookCategory)request.Category);

                // Add to repository (stages the change)
                await bookRepository.AddAsync(book, cancellationToken);

                // Save through UnitOfWork
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return success result
                var bookDto = mapper.Map<BookDto>(book);

                logger.LogInformation("Successfully created book with ID: {BookId}", book.BookId.Value);

                return Result<BookDto>.Success(bookDto);
            }
            catch (DuplicateBookException ex)
            {
                logger.LogWarning(ex, "Duplicate book creation attempted");
                return Result<BookDto>.Failure(DomainErrors.Book.AlreadyExists(request.Title, request.PublicationYear));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("title"))
            {
                logger.LogWarning(ex, "Invalid title provided");
                return Result<BookDto>.Failure(DomainErrors.Book.InvalidTitle());
            }
            catch (ArgumentException ex) when (ex.Message.Contains("author"))
            {
                logger.LogWarning(ex, "Invalid author provided");
                return Result<BookDto>.Failure(DomainErrors.Book.InvalidAuthor());
            }
            catch (ArgumentException ex) when (ex.Message.Contains("publication"))
            {
                logger.LogWarning(ex, "Invalid publication year provided");
                return Result<BookDto>.Failure(DomainErrors.Book.InvalidPublicationYear());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating book: {Title} by {Author}", request.Title, request.Author);
                return Result<BookDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
