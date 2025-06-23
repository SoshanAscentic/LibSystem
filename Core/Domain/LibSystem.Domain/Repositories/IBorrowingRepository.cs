using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Repositories
{
    public interface IBorrowingRepository
    {
        // Basic CRUD operations
        Task<BorrowingRecord?> GetByIdAsync(BorrowingId id, CancellationToken cancellationToken = default);
        Task<BorrowingRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BorrowingRecord>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(BorrowingRecord borrowingRecord, CancellationToken cancellationToken = default);
        void Update(BorrowingRecord borrowingRecord);
        void Remove(BorrowingRecord borrowingRecord);

        // Domain-specific queries
        Task<BorrowingRecord?> GetActiveBorrowingAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BorrowingRecord>> GetActiveBorrowingsByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BorrowingRecord>> GetBorrowingHistoryByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BorrowingRecord>> GetOverdueBorrowingsAsync(CancellationToken cancellationToken = default);
        Task<bool> IsBookCurrentlyBorrowedAsync(BookId bookId, CancellationToken cancellationToken = default);
        Task<bool> HasMemberBorrowedBookAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default);
    }
}
