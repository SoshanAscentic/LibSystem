using AutoMapper;
using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.DTOs.Book;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Books.GetAllBooks
{
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
                logger.LogInformation("Retrieving all books");

                var books = await bookRepository.GetAllAsync(cancellationToken);
                var bookDtos = mapper.Map<IReadOnlyList<BookDto>>(books);

                logger.LogInformation("Successfully retrieved {Count} books", bookDtos.Count);

                return Result<IReadOnlyList<BookDto>>.Success(bookDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving all books");
                return Result<IReadOnlyList<BookDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
