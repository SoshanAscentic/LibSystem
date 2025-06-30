using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.DTOs;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.GetBookById
{
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
                if (request.BookId <= 0)
                {
                    return Result<BookDto>.Failure(DomainErrors.General.InvalidId("Book"));
                }

                logger.LogInformation("Retrieving book with ID: {BookId}", request.BookId);

                var bookId = BookId.Create(request.BookId);
                var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);

                if (book == null)
                {
                    logger.LogWarning("Book not found: {BookId}", request.BookId);
                    return Result<BookDto>.Failure(DomainErrors.Book.NotFound(request.BookId));
                }

                var bookDto = mapper.Map<BookDto>(book);

                logger.LogInformation("Successfully retrieved book: {Title}", book.Title);

                return Result<BookDto>.Success(bookDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving book with ID: {BookId}", request.BookId);
                return Result<BookDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
