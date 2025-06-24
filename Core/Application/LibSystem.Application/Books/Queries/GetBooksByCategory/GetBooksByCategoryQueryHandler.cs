using AutoMapper;
using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Queries.GetBooksByCategory
{
    public class GetBooksByCategoryQueryHandler : IRequestHandler<GetBooksByCategoryQuery, Result<IReadOnlyList<BookDto>>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBooksByCategoryQueryHandler> _logger;

        public GetBooksByCategoryQueryHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<GetBooksByCategoryQueryHandler> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<BookDto>>> Handle(GetBooksByCategoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Category))
                {
                    return Result<IReadOnlyList<BookDto>>.Failure("Category cannot be empty.");
                }

                _logger.LogInformation("Retrieving books by category: {Category}", request.Category);

                // Validate and parse category
                if (!Enum.TryParse<Book.BookCategory>(request.Category, true, out var bookCategory))
                {
                    var error = $"Invalid category: {request.Category}. Valid categories are: Fiction, History, Child";
                    _logger.LogWarning(error);
                    return Result<IReadOnlyList<BookDto>>.Failure(error);
                }

                var books = await _bookRepository.GetBooksByCategoryAsync(bookCategory, cancellationToken);
                var bookDtos = _mapper.Map<IReadOnlyList<BookDto>>(books);

                _logger.LogInformation("Successfully retrieved {Count} books in category: {Category}", bookDtos.Count, request.Category);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by category: {Category}", request.Category);
                return Result<IReadOnlyList<BookDto>>.Failure("An error occurred while retrieving books by category.");
            }
        }
    }
}
