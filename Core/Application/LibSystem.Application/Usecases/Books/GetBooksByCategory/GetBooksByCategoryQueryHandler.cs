using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.DTOs;
using LibSystem.Domain.Entities.Books;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.GetBooksByCategory
{
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
                if (string.IsNullOrWhiteSpace(request.Category))
                {
                    return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidCategory());
                }

                logger.LogInformation("Retrieving books by category: {Category}", request.Category);

                // Validate and parse category
                if (!Enum.TryParse<Book.BookCategory>(request.Category, true, out var bookCategory))
                {
                    logger.LogWarning("Invalid category provided: {Category}", request.Category);
                    return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidCategory());
                }

                var books = await bookRepository.GetBooksByCategoryAsync(bookCategory, cancellationToken);
                var bookDtos = mapper.Map<IReadOnlyList<BookDto>>(books);

                logger.LogInformation("Successfully retrieved {Count} books in category: {Category}", bookDtos.Count, request.Category);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("category") || ex.Message.Contains("Category"))
            {
                logger.LogWarning(ex, "Invalid category provided: {Category}", request.Category);
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.Book.InvalidCategory());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving books by category: {Category}", request.Category);
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
