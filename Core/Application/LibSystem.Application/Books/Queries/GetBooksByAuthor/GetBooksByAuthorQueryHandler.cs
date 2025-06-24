using AutoMapper;
using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using LibSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Queries.GetBooksByAuthor
{
    public class GetBooksByAuthorQueryHandler : IRequestHandler<GetBooksByAuthorQuery, Result<IReadOnlyList<BookDto>>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBooksByAuthorQueryHandler> _logger;

        public GetBooksByAuthorQueryHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<GetBooksByAuthorQueryHandler> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<BookDto>>> Handle(GetBooksByAuthorQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Author))
                {
                    return Result<IReadOnlyList<BookDto>>.Failure("Author name cannot be empty.");
                }

                _logger.LogInformation("Retrieving books by author: {Author}", request.Author);

                var books = await _bookRepository.GetBooksByAuthorAsync(request.Author.Trim(), cancellationToken);
                var bookDtos = _mapper.Map<IReadOnlyList<BookDto>>(books);

                _logger.LogInformation("Successfully retrieved {Count} books by author: {Author}", bookDtos.Count, request.Author);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by author: {Author}", request.Author);
                return Result<IReadOnlyList<BookDto>>.Failure("An error occurred while retrieving books by author.");
            }
        }
    }
}
