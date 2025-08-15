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
                // Validate input parameters
                if (request.BookId <= 0)
                {
                    return Result<string>.Failure(DomainErrors.General.InvalidId("Book"));
                }

                if (request.MemberID <= 0)
                {
                    return Result<string>.Failure(DomainErrors.General.InvalidId("Member"));
                }

                logger.LogInformation("Processing return request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);

                var bookId = BookId.Create(request.BookId);
                var memberId = MemberId.Create(request.MemberID);

                // Use UnitOfWork's execution strategy to handle the entire operation within a transaction
                var result = await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Get and validate entities
                    var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
                    if (book == null)
                    {
                        logger.LogWarning("Book not found for return: {BookId}", request.BookId);
                        throw new BookNotFoundException(request.BookId);
                    }

                    var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);
                    if (member == null)
                    {
                        logger.LogWarning("Member not found for return: {MemberID}", request.MemberID);
                        throw new MemberNotFoundException(request.MemberID);
                    }

                    // Find active borrowing record
                    var borrowingRecord = await borrowingRepository.GetActiveBorrowingAsync(bookId, memberId, cancellationToken);
                    if (borrowingRecord == null)
                    {
                        logger.LogWarning("No active borrowing found for return - Book: {BookId}, Member: {MemberID}",
                            request.BookId, request.MemberID);
                        throw new InvalidBorrowingException($"Book {request.BookId} is not currently borrowed by member {request.MemberID}.");
                    }

                    // Execute business logic on domain entities
                    member.ReturnBook(bookId);                    // This validates return rules
                    book.Return(memberId, borrowingRecord.BorrowedAt); // This validates book state
                    borrowingRecord.MarkAsReturned();            // This updates borrowing record

                    // Update entities (stages changes)
                    memberRepository.Update(member);
                    bookRepository.Update(book);
                    borrowingRepository.Update(borrowingRecord);

                    // Save all changes through UnitOfWork
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    var successMessage = $"Book '{book.Title}' returned successfully by {member.Name.Value}!";
                    logger.LogInformation("Successfully processed return request - Book: {BookId}, Member: {MemberID}",
                        request.BookId, request.MemberID);

                    return successMessage;
                }, cancellationToken);

                return Result<string>.Success(result);
            }
            catch (InvalidBorrowingException ex) when (ex.Message.Contains("not currently borrowed") || ex.Message.Contains("not borrowed"))
            {
                logger.LogWarning(ex, "Book not currently borrowed by member");
                return Result<string>.Failure(DomainErrors.Borrowing.BookNotBorrowedByMember(request.BookId, request.MemberID));
            }
            catch (InvalidBorrowingException ex) when (ex.Message.Contains("already returned") || ex.Message.Contains("returned"))
            {
                logger.LogWarning(ex, "Book already returned");
                return Result<string>.Failure(DomainErrors.Borrowing.BookAlreadyReturned(request.BookId));
            }
            catch (InvalidBorrowingException ex)
            {
                logger.LogWarning(ex, "Invalid borrowing operation during return");
                return Result<string>.Failure(DomainErrors.Borrowing.InvalidBorrowingOperation());
            }
            catch (BookNotFoundException ex)
            {
                logger.LogWarning(ex, "Book not found during return operation");
                return Result<string>.Failure(DomainErrors.Book.NotFound(request.BookId));
            }
            catch (MemberNotFoundException ex)
            {
                logger.LogWarning(ex, "Member not found during return operation");
                return Result<string>.Failure(DomainErrors.Member.NotFound(request.MemberID));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing return request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);
                return Result<string>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
