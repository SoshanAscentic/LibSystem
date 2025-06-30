using LibSystem.Application.Contracts.Repositories;
using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.ValueObjects;
using LibSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence.Repositories
{
    public class BorrowingRepository : GenericRepository<BorrowingRecord>, IBorrowingRepository
    {
        public BorrowingRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<BorrowingRecord?> GetByIdAsync(BorrowingId id, CancellationToken cancellationToken = default)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            // Use base Id for querying
            return await dbSet.FindAsync(new object[] { id.Value }, cancellationToken);
        }

        public async Task<BorrowingRecord?> GetActiveBorrowingAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (bookId == null) throw new ArgumentNullException(nameof(bookId));
            if (memberId == null) throw new ArgumentNullException(nameof(memberId));

            return await dbSet
                .FirstOrDefaultAsync(br =>
                    br.BookId.Value == bookId.Value &&
                    br.MemberId.Value == memberId.Value &&
                    br.ReturnedAt == null,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<BorrowingRecord>> GetActiveBorrowingsByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (memberId == null) throw new ArgumentNullException(nameof(memberId));

            return await dbSet
                .Where(br => br.MemberId.Value == memberId.Value && br.ReturnedAt == null)
                .OrderBy(br => br.BorrowedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BorrowingRecord>> GetBorrowingHistoryByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (memberId == null) throw new ArgumentNullException(nameof(memberId));

            return await dbSet
                .Where(br => br.MemberId.Value == memberId.Value)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BorrowingRecord>> GetOverdueBorrowingsAsync(CancellationToken cancellationToken = default)
        {
            // Get all active borrowings first
            var activeBorrowings = await dbSet
                .Where(br => br.ReturnedAt == null)
                .ToListAsync(cancellationToken);

            // Filter using domain logic for overdue calculation (client-side)
            var overdueBorrowings = activeBorrowings
                .Where(borrowing => borrowing.IsOverdue())
                .OrderBy(br => br.BorrowedAt)
                .ToList();

            return overdueBorrowings;
        }

        public async Task<bool> IsBookCurrentlyBorrowedAsync(BookId bookId, CancellationToken cancellationToken = default)
        {
            if (bookId == null) return false;

            return await dbSet
                .AnyAsync(br => br.BookId.Value == bookId.Value && br.ReturnedAt == null, cancellationToken);
        }

        public async Task<bool> HasMemberBorrowedBookAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (bookId == null || memberId == null) return false;

            return await dbSet
                .AnyAsync(br => br.BookId.Value == bookId.Value && br.MemberId.Value == memberId.Value,
                    cancellationToken);
        }

        public override async Task<IReadOnlyList<BorrowingRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet
                .OrderByDescending(br => br.BorrowedAt)
                .ThenBy(br => br.Id) // Use base Id for secondary ordering
                .ToListAsync(cancellationToken);
        }

        public override async Task AddAsync(BorrowingRecord entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await base.AddAsync(entity, cancellationToken);
        }
    }
}
