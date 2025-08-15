// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBookByIdQueryHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetBookById
{
    using AutoMapper;
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.DTOs.Book;
    using LibSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetBookByIdQueryHandler> logger;

        public GetBookByIdQueryHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<GetBookByIdQueryHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<BookDto>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.bookId <= 0)
                {
                    return Result<BookDto>.Failure(DomainErrors.General.InvalidId("Book"));
                }

                this.logger.LogInformation("Retrieving book with ID: {BookId}", request.bookId);

                var bookId = BookId.Create(request.bookId);
                var book = await this.bookRepository.GetByIdAsync(bookId, cancellationToken);

                if (book == null)
                {
                    this.logger.LogWarning("Book not found: {BookId}", request.bookId);
                    return Result<BookDto>.Failure(DomainErrors.Book.NotFound(request.bookId));
                }

                var bookDto = this.mapper.Map<BookDto>(book);

                this.logger.LogInformation("Successfully retrieved book: {Title}", book.Title);

                return Result<BookDto>.Success(bookDto);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving book with ID: {BookId}", request.bookId);
                return Result<BookDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
