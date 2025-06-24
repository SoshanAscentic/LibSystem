using LibSystem.Application.Common.Models;
using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.Repositories;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Borrowing.Commands.BorrowBook
{
    public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Result<string>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IBorrowingRepository borrowingRepository;
        private readonly ILogger<BorrowBookCommandHandler> logger;

        public BorrowBookCommandHandler(
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            IBorrowingRepository borrowingRepository,
            ILogger<BorrowBookCommandHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.memberRepository = memberRepository;
            this.borrowingRepository = borrowingRepository;
            this.logger = logger;
        }

        public async Task<Result<string>> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Processing borrow request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);

                // Create value objects
                var bookId = BookId.Create(request.BookId);
                var memberId = MemberId.Create(request.MemberID);

                // Start transaction for consistency
                await bookRepository.BeginTransactionAsync();

                try
                {
                    // Get and validate entities
                    var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
                    if (book == null)
                    {
                        var error = $"Book with ID {request.BookId} not found.";
                        logger.LogWarning(error);
                        return Result<string>.Failure(error);
                    }

                    var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);
                    if (member == null)
                    {
                        var error = $"Member with ID {request.MemberID} not found.";
                        logger.LogWarning(error);
                        return Result<string>.Failure(error);
                    }

                    // Execute business logic on domain entities
                    member.BorrowBook(bookId); // This validates borrowing rules
                    book.Borrow(memberId);     // This validates book availability

                    // Create borrowing record
                    var borrowingRecord = BorrowingRecord.Create(bookId, memberId);
                    await borrowingRepository.AddAsync(borrowingRecord, cancellationToken);

                    // Update entities
                    memberRepository.Update(member);
                    bookRepository.Update(book);

                    // Save all changes
                    await bookRepository.SaveChangesAsync(cancellationToken);

                    // Commit transaction
                    await bookRepository.CommitTransactionAsync();

                    var successMessage = $"Book '{book.Title}' borrowed successfully by {member.Name.Value}!";
                    logger.LogInformation("Successfully processed borrow request - Book: {BookId}, Member: {MemberID}",
                        request.BookId, request.MemberID);

                    return Result<string>.Success(successMessage);
                }
                catch
                {
                    await bookRepository.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (InvalidBorrowingException ex)
            {
                logger.LogWarning(ex, "Business rule violation during borrow operation");
                return Result<string>.Failure(ex.Message);
            }
            catch (BookNotFoundException ex)
            {
                logger.LogWarning(ex, "Book not found during borrow operation");
                return Result<string>.Failure(ex.Message);
            }
            catch (MemberNotFoundException ex)
            {
                logger.LogWarning(ex, "Member not found during borrow operation");
                return Result<string>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing borrow request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);
                return Result<string>.Failure("An error occurred while borrowing the book.");
            }
        }
    }
}
