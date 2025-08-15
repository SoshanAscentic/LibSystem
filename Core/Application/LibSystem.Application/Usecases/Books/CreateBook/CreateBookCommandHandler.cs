// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateBookCommandHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.CreateBook
{
    using AutoMapper;
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.Contracts.UoW;
    using LibSystem.Application.DTOs.Book;
    using LibSystem.Domain.Exceptions;
    using MediatR;
    using Microsoft.Extensions.Logging;

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
                this.logger.LogInformation("Creating book: {Title} by {Author}", request.title, request.author);

                // Check for duplicates (business rule: unique title + year combination)
                var existingBook = await this.bookRepository.GetByTitleAndYearAsync(
                    request.title, request.publicationYear, cancellationToken);

                if (existingBook != null)
                {
                    this.logger.LogWarning("Duplicate book creation attempted: {Title} ({Year})", request.title, request.publicationYear);
                    return Result<BookDto>.Failure(DomainErrors.Book.AlreadyExists(request.title, request.publicationYear));
                }

                // Create new book using domain factory method
                var book = Book.Create(
                    request.title,
                    request.author,
                    request.publicationYear,
                    (Book.BookCategory)request.category);

                // Add to repository (stages the change)
                await this.bookRepository.AddAsync(book, cancellationToken);

                // Save through UnitOfWork
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return success result
                var bookDto = this.mapper.Map<BookDto>(book);

                this.logger.LogInformation("Successfully created book with ID: {BookId}", book.BookId.Value);

                return Result<BookDto>.Success(bookDto);
            }
            catch (DuplicateBookException ex)
            {
                this.logger.LogWarning(ex, "Duplicate book creation attempted");
                return Result<BookDto>.Failure(DomainErrors.Book.AlreadyExists(request.title, request.publicationYear));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("title"))
            {
                this.logger.LogWarning(ex, "Invalid title provided");
                return Result<BookDto>.Failure(DomainErrors.Book.InvalidTitle());
            }
            catch (ArgumentException ex) when (ex.Message.Contains("author"))
            {
                this.logger.LogWarning(ex, "Invalid author provided");
                return Result<BookDto>.Failure(DomainErrors.Book.InvalidAuthor());
            }
            catch (ArgumentException ex) when (ex.Message.Contains("publication"))
            {
                this.logger.LogWarning(ex, "Invalid publication year provided");
                return Result<BookDto>.Failure(DomainErrors.Book.InvalidPublicationYear());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating book: {Title} by {Author}", request.title, request.author);
                return Result<BookDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
