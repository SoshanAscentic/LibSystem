using AutoMapper;
using LibSystem.Application.Books.DTOs;
using LibSystem.Application.Common.Models;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Books.Commands.CreateBook
{
    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Result<BookDto>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateBookCommandHandler> _logger;

        public CreateBookCommandHandler(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<CreateBookCommandHandler> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<BookDto>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating book: {Title} by {Author}", request.Title, request.Author);

                // Check for duplicates (business rule: unique title + year combination)
                var existingBook = await _bookRepository.GetByTitleAndYearAsync(
                    request.Title, request.PublicationYear, cancellationToken);

                if (existingBook != null)
                {
                    var error = $"A book with title '{request.Title}' and publication year {request.PublicationYear} already exists.";
                    _logger.LogWarning(error);
                    return Result<BookDto>.Failure(error);
                }

                // Create new book using domain factory method
                var book = Book.Create(
                    request.Title,
                    request.Author,
                    request.PublicationYear,
                    (Book.BookCategory)request.Category);

                // Save to repository
                await _bookRepository.AddAsync(book, cancellationToken);
                await _bookRepository.SaveChangesAsync(cancellationToken);

                // Map to DTO and return success result
                var bookDto = _mapper.Map<BookDto>(book);

                _logger.LogInformation("Successfully created book with ID: {BookId}", book.BookId.Value);

                return Result<BookDto>.Success(bookDto);
            }
            catch (DuplicateBookException ex)
            {
                _logger.LogWarning(ex, "Duplicate book creation attempted");
                return Result<BookDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book: {Title} by {Author}", request.Title, request.Author);
                return Result<BookDto>.Failure("An error occurred while creating the book.");
            }
        }
    }
}
