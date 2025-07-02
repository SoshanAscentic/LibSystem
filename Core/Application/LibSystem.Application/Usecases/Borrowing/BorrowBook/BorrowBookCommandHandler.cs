// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorrowBookCommandHandler.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Usecases.Borrowing.BorrowBook
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.Contracts.Repositories;
    using LibSystem.Application.Contracts.UoW;
    using LibSystem.Domain.Entities.Borrowing;
    using LibSystem.Domain.Exceptions;
    using LibSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Result<string>>
    {
        private readonly IBookRepository bookRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IBorrowingRepository borrowingRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<BorrowBookCommandHandler> logger;

        public BorrowBookCommandHandler(
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            IBorrowingRepository borrowingRepository,
            IUnitOfWork unitOfWork,
            ILogger<BorrowBookCommandHandler> logger)
        {
            this.bookRepository = bookRepository;
            this.memberRepository = memberRepository;
            this.borrowingRepository = borrowingRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<string>> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Processing borrow request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);

                if (request.BookId <= 0)
                    return Result<string>.Failure(DomainErrors.General.InvalidId("Book"));

                if (request.MemberID <= 0)
                    return Result<string>.Failure(DomainErrors.General.InvalidId("Member"));

                var bookId = BookId.Create(request.BookId);
                var memberId = MemberId.Create(request.MemberID);

                // Use UnitOfWork's execution strategy to handle the entire operation within a transaction
                var result = await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Get and validate entities
                    var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
                    if (book == null)
                    {
                        logger.LogWarning("Book not found: {BookId}", request.BookId);
                        throw new BookNotFoundException(request.BookId);
                    }

                    var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);
                    if (member == null)
                    {
                        logger.LogWarning("Member not found: {MemberID}", request.MemberID);
                        throw new MemberNotFoundException(request.MemberID);
                    }

                    // Execute business logic on domain entities
                    member.BorrowBook(bookId); // This validates borrowing rules
                    book.Borrow(memberId);     // This validates book availability

                    // Create borrowing record
                    var borrowingRecord = BorrowingRecord.Create(bookId, memberId);
                    await borrowingRepository.AddAsync(borrowingRecord, cancellationToken);

                    // Update entities (stages changes)
                    memberRepository.Update(member);
                    bookRepository.Update(book);

                    // Save all changes through UnitOfWork
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    var successMessage = $"Book '{book.Title}' borrowed successfully by {member.Name.Value}!";
                    logger.LogInformation("Successfully processed borrow request - Book: {BookId}, Member: {MemberID}",
                        request.BookId, request.MemberID);

                    return successMessage;
                }, cancellationToken);

                return Result<string>.Success(result);
            }
            catch (InvalidBorrowingException ex) when (ex.Message.Contains("not available"))
            {
                logger.LogWarning(ex, "Book not available for borrowing");
                return Result<string>.Failure(DomainErrors.Book.NotAvailable(request.BookId));
            }
            catch (InvalidBorrowingException ex) when (ex.Message.Contains("limit"))
            {
                logger.LogWarning(ex, "Member borrowing limit exceeded");
                return Result<string>.Failure(DomainErrors.Member.BorrowingLimitExceeded(request.MemberID, 0, 5)); // You'd get actual values from member
            }
            catch (InvalidBorrowingException ex) when (ex.Message.Contains("permission"))
            {
                logger.LogWarning(ex, "Member cannot borrow books");
                return Result<string>.Failure(DomainErrors.Member.CannotBorrow(request.MemberID, "Unknown")); // You'd get actual member type
            }
            catch (BookNotFoundException ex)
            {
                logger.LogWarning(ex, "Book not found during borrow operation");
                return Result<string>.Failure(DomainErrors.Book.NotFound(request.BookId));
            }
            catch (MemberNotFoundException ex)
            {
                logger.LogWarning(ex, "Member not found during borrow operation");
                return Result<string>.Failure(DomainErrors.Member.NotFound(request.MemberID));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing borrow request - Book: {BookId}, Member: {MemberID}",
                    request.BookId, request.MemberID);
                return Result<string>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
