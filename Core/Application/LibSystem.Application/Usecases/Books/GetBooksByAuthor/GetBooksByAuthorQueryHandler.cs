// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBooksByAuthorQueryHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetBooksByAuthor
{
    using AutoMapper;
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.DTOs.Book;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetBooksByAuthorQueryHandler : IRequestHandler<GetBooksByAuthorQuery, Result<IReadOnlyList<BookDto>>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetBooksByAuthorQueryHandler> logger;

        public GetBooksByAuthorQueryHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<GetBooksByAuthorQueryHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<IReadOnlyList<BookDto>>> Handle(GetBooksByAuthorQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.author))
                {
                    return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidAuthor());
                }

                this.logger.LogInformation("Retrieving books by author: {Author}", request.author);

                var books = await this.bookRepository.GetBooksByAuthorAsync(request.author, cancellationToken);
                var bookDtos = this.mapper.Map<IReadOnlyList<BookDto>>(books);

                this.logger.LogInformation("Successfully retrieved {Count} books by author: {Author}", bookDtos.Count, request.author);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("author") || ex.Message.Contains("Author"))
            {
                this.logger.LogWarning(ex, "Invalid author provided: {Author}", request.author);
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidAuthor());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error retrieving books by author: {Author}", request.author);
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
