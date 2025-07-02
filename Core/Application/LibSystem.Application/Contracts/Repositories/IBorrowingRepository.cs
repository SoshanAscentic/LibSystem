// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBorrowingRepository.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.Repositories
{
    using LibSystem.Domain.Common;
    using LibSystem.Domain.Entities.Borrowing;
    using LibSystem.Domain.ValueObjects;

    public interface IBorrowingRepository : IGenericRepository<BorrowingRecord>
    {
        // Borrowing-specific operations
        Task<BorrowingRecord?> GetByIdAsync(BorrowingId id, CancellationToken cancellationToken = default);

        Task<BorrowingRecord?> GetActiveBorrowingAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BorrowingRecord>> GetActiveBorrowingsByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BorrowingRecord>> GetBorrowingHistoryByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BorrowingRecord>> GetOverdueBorrowingsAsync(CancellationToken cancellationToken = default);

        Task<bool> IsBookCurrentlyBorrowedAsync(BookId bookId, CancellationToken cancellationToken = default);

        Task<bool> HasMemberBorrowedBookAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default);
    }
}
