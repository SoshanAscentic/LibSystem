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

namespace LibSystem.Application.Usecases.Borrowing.GetMemberBorrowingStatus
{
    public class GetMemberBorrowingStatusQueryHandler : IRequestHandler<GetMemberBorrowingStatusQuery, Result<BorrowingStatusDto>>
    {
        private readonly IMemberRepository memberRepository;
        private readonly IBookRepository bookRepository;
        private readonly IBorrowingRepository borrowingRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetMemberBorrowingStatusQueryHandler> logger;

        public GetMemberBorrowingStatusQueryHandler(
            IMemberRepository memberRepository,
            IBookRepository bookRepository,
            IBorrowingRepository borrowingRepository,
            IMapper mapper,
            ILogger<GetMemberBorrowingStatusQueryHandler> logger)
        {
            this.memberRepository = memberRepository;
            this.bookRepository = bookRepository;
            this.borrowingRepository = borrowingRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<BorrowingStatusDto>> Handle(GetMemberBorrowingStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.MemberId <= 0)
                {
                    return Result<BorrowingStatusDto>.Failure(DomainErrors.General.InvalidId("Member"));
                }

                logger.LogInformation("Retrieving borrowing status for member: {MemberId}", request.MemberId);

                var memberId = MemberId.Create(request.MemberId);
                var member = await memberRepository.GetByIdAsync(memberId, cancellationToken);

                if (member == null)
                {
                    logger.LogWarning("Member not found for borrowing status: {MemberId}", request.MemberId);
                    return Result<BorrowingStatusDto>.Failure(DomainErrors.Member.NotFound(request.MemberId));
                }

                // Get borrowing status
                var borrowingStatus = mapper.Map<BorrowingStatusDto>(member);

                // Get active borrowing records
                var activeBorrowings = await borrowingRepository.GetActiveBorrowingsByMemberAsync(memberId, cancellationToken);

                // Populate borrowed books information
                borrowingStatus.BorrowedBooks = new List<BorrowedBookDto>();

                foreach (var borrowing in activeBorrowings)
                {
                    var book = await bookRepository.GetByIdAsync(borrowing.BookId, cancellationToken);
                    if (book != null)
                    {
                        var borrowedBook = new BorrowedBookDto
                        {
                            BookId = borrowing.BookId.Value,
                            Title = book.Title,
                            Author = book.Author,
                            BorrowedAt = borrowing.BorrowedAt,
                            DaysBorrowed = borrowing.DaysBorrowed,
                            IsOverdue = borrowing.IsOverdue()
                        };
                        borrowingStatus.BorrowedBooks.Add(borrowedBook);
                    }
                }

                logger.LogInformation("Successfully retrieved borrowing status for member: {MemberName} ({MemberId}) - {BorrowedCount} books borrowed",
                    member.Name.Value, member.MemberId.Value, borrowingStatus.BorrowedBooks.Count);

                return Result<BorrowingStatusDto>.Success(borrowingStatus);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID") || ex.Message.Contains("positive"))
            {
                logger.LogWarning(ex, "Invalid member ID provided: {MemberId}", request.MemberId);
                return Result<BorrowingStatusDto>.Failure(DomainErrors.General.InvalidId("Member"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error retrieving borrowing status for member: {MemberId}", request.MemberId);
                return Result<BorrowingStatusDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}
