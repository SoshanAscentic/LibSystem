// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBooksByCategoryQueryHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Books.GetBooksByCategory
{
    using AutoMapper;
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.DTOs.Book;
    using LibSystem.Domain.Entities.Books;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetBooksByCategoryQueryHandler : IRequestHandler<GetBooksByCategoryQuery, Result<IReadOnlyList<BookDto>>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetBooksByCategoryQueryHandler> logger;

        public GetBooksByCategoryQueryHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<GetBooksByCategoryQueryHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<IReadOnlyList<BookDto>>> Handle(GetBooksByCategoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.category))
                {
                    return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidCategory());
                }

                this.logger.LogInformation("Retrieving books by category: {Category}", request.category);

                // Validate and parse category
                if (!Enum.TryParse<Book.BookCategory>(request.category, true, out var bookCategory))
                {
                    this.logger.LogWarning("Invalid category provided: {Category}", request.category);
                    return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidCategory());
                }

                var books = await this.bookRepository.GetBooksByCategoryAsync(bookCategory, cancellationToken);
                var bookDtos = this.mapper.Map<IReadOnlyList<BookDto>>(books);

                this.logger.LogInformation("Successfully retrieved {Count} books in category: {Category}", bookDtos.Count, request.category);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("category") || ex.Message.Contains("Category"))
            {
                this.logger.LogWarning(ex, "Invalid category provided: {Category}", request.category);
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidCategory());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error retrieving books by category: {Category}", request.category);
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
