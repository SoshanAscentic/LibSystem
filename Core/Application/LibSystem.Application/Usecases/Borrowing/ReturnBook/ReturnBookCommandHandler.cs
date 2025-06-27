using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.Contracts.UoW;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Usecases.Borrowing.ReturnBook
{
    public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, Result<string>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IBorrowingRepository borrowingRepository;
        private readonly IUnitOfWork unitOfWork; 
        private readonly ILogger<ReturnBookCommandHandler> logger;

        public ReturnBookCommandHandler(
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            IBorrowingRepository borrowingRepository,
            IUnitOfWork unitOfWork,
            ILogger<ReturnBookCommandHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.memberRepository = memberRepository;
            this.borrowingRepository = borrowingRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<string>> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Processing return request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);

                var bookId = BookId.Create(request.BookId);
                var memberId = MemberId.Create(request.MemberID);

                // Use UnitOfWork for transaction management
                await unitOfWork.BeginTransactionAsync();

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

                    // Find active borrowing record
                    var borrowingRecord = await borrowingRepository.GetActiveBorrowingAsync(bookId, memberId, cancellationToken);
                    if (borrowingRecord == null)
                    {
                        var error = $"No active borrowing found for book {request.BookId} by member {request.MemberID}.";
                        logger.LogWarning(error);
                        return Result<string>.Failure(error);
                    }

                    // Execute business logic on domain entities
                    member.ReturnBook(bookId);                    // This validates return rules
                    book.Return(memberId, borrowingRecord.BorrowedAt); // This validates book state
                    borrowingRecord.MarkAsReturned();            // This updates borrowing record

                    // Update entities (stages changes)
                    memberRepository.Update(member);
                    bookRepository.Update(book);
                    borrowingRepository.Update(borrowingRecord);

                    //  Save all changes through UnitOfWork
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    // Commit transaction through UnitOfWork
                    await unitOfWork.CommitTransactionAsync();

                    var successMessage = $"Book '{book.Title}' returned successfully by {member.Name.Value}!";
                    logger.LogInformation("Successfully processed return request - Book: {BookId}, Member: {MemberID}",
                        request.BookId, request.MemberID);

                    return Result<string>.Success(successMessage);
                }
                catch
                {
                    // Rollback through UnitOfWork
                    await unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (InvalidBorrowingException ex)
            {
                logger.LogWarning(ex, "Business rule violation during return operation");
                return Result<string>.Failure(ex.Message);
            }
            catch (BookNotFoundException ex)
            {
                logger.LogWarning(ex, "Book not found during return operation");
                return Result<string>.Failure(ex.Message);
            }
            catch (MemberNotFoundException ex)
            {
                logger.LogWarning(ex, "Member not found during return operation");
                return Result<string>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing return request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);
                return Result<string>.Failure("An error occurred while returning the book.");
            }
        }
    }
}
