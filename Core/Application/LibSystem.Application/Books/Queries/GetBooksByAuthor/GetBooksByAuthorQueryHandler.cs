using AutoMapper;
using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibSystem.Application.Books.Queries.GetBooksByAuthor
{
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
                if (string.IsNullOrWhiteSpace(request.Author))
                {
                    return Result<IReadOnlyList<BookDto>>.Failure("Author name cannot be empty.");
                }

                logger.LogInformation("Retrieving books by author: {Author}", request.Author);

                var books = await bookRepository.GetBooksByAuthorAsync(request.Author, cancellationToken);
                var bookDtos = mapper.Map<IReadOnlyList<BookDto>>(books);

                logger.LogInformation("Successfully retrieved {Count} books by author: {Author}", bookDtos.Count, request.Author);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving books by author: {Author}", request.Author);
                return Result<IReadOnlyList<BookDto>>.Failure("An error occurred while retrieving books by author.");
            }
        }
    }
}