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

            // Use raw int values instead of .Value to avoid LINQ translation issues
            var bookIdValue = bookId.Value;
            var memberIdValue = memberId.Value;

            // Get all borrowing records with matching criteria, then filter on client side if needed
            var borrowingRecords = await dbSet
                .Where(br => br.ReturnedAt == null)
                .ToListAsync(cancellationToken);

            // Filter by BookId and MemberId on client side using domain logic
            return borrowingRecords.FirstOrDefault(br =>
                br.BookId.Value == bookIdValue &&
                br.MemberId.Value == memberIdValue);
        }

        public async Task<IReadOnlyList<BorrowingRecord>> GetActiveBorrowingsByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (memberId == null) throw new ArgumentNullException(nameof(memberId));

            // Get all active borrowings first, then filter on client side
            var activeBorrowings = await dbSet
                .Where(br => br.ReturnedAt == null)
                .OrderBy(br => br.BorrowedAt)
                .ToListAsync(cancellationToken);

            // Filter by MemberId using domain logic (client-side)
            var memberIdValue = memberId.Value;
            return activeBorrowings
                .Where(br => br.MemberId.Value == memberIdValue)
                .ToList();
        }

        public async Task<IReadOnlyList<BorrowingRecord>> GetBorrowingHistoryByMemberAsync(MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (memberId == null) throw new ArgumentNullException(nameof(memberId));

            // Get all borrowing records first, then filter on client side
            var allBorrowings = await dbSet
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync(cancellationToken);

            // Filter by MemberId using domain logic (client-side)
            var memberIdValue = memberId.Value;
            return allBorrowings
                .Where(br => br.MemberId.Value == memberIdValue)
                .ToList();
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

            // Get all active borrowings and check on client side
            var activeBorrowings = await dbSet
                .Where(br => br.ReturnedAt == null)
                .ToListAsync(cancellationToken);

            var bookIdValue = bookId.Value;
            return activeBorrowings.Any(br => br.BookId.Value == bookIdValue);
        }

        public async Task<bool> HasMemberBorrowedBookAsync(BookId bookId, MemberId memberId, CancellationToken cancellationToken = default)
        {
            if (bookId == null || memberId == null) return false;

            // Get all borrowing records and check on client side
            var allBorrowings = await dbSet.ToListAsync(cancellationToken);

            var bookIdValue = bookId.Value;
            var memberIdValue = memberId.Value;
            return allBorrowings.Any(br =>
                br.BookId.Value == bookIdValue &&
                br.MemberId.Value == memberIdValue);
        }

        public async Task<int> GetNextBorrowingIdAsync(CancellationToken cancellationToken = default)
        {
            // This method is no longer used for pre-assignment
            // We let EF Core handle the ID assignment and sync afterward
            var lastBorrowing = await dbSet
                .OrderByDescending(br => br.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return lastBorrowing?.Id + 1 ?? 1;
        }

        public override async Task<IReadOnlyList<BorrowingRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet
                .OrderByDescending(br => br.BorrowedAt)
                .ThenBy(br => br.Id)
                .ToListAsync(cancellationToken);
        }

        public override async Task AddAsync(BorrowingRecord entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // FIXED: Don't pre-assign BorrowingId - let EF Core assign the database Id first
            // The BorrowingId will be synced with the database Id after SaveChanges in UnitOfWork

            // Ensure BorrowingId is set to default so UnitOfWork knows to sync it
            if (entity.BorrowingId.Value != 0)
            {
                // Reset to default so sync will happen
                var borrowingIdProperty = entity.GetType().GetProperty("BorrowingId");
                borrowingIdProperty?.SetValue(entity, BorrowingId.CreateNew());
            }

            await base.AddAsync(entity, cancellationToken);
        }

        public override void Update(BorrowingRecord entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // FIXED: Only sync BorrowingId if it's different from database Id
            if (entity.Id > 0 && entity.BorrowingId.Value != entity.Id)
            {
                var borrowingIdProperty = entity.GetType().GetProperty("BorrowingId");
                borrowingIdProperty?.SetValue(entity, BorrowingId.Create(entity.Id));
            }

            base.Update(entity);
        }
    }
}
