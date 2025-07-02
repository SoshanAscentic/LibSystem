// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAllBooksQueryHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetAllBooks
{
    using AutoMapper;
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.DTOs.Book;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, Result<IReadOnlyList<BookDto>>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetAllBooksQueryHandler> logger;

        public GetAllBooksQueryHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<GetAllBooksQueryHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<IReadOnlyList<BookDto>>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Retrieving all books");

                var books = await this.bookRepository.GetAllAsync(cancellationToken);
                var bookDtos = this.mapper.Map<IReadOnlyList<BookDto>>(books);

                this.logger.LogInformation("Successfully retrieved {Count} books", bookDtos.Count);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error retrieving all books");
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
