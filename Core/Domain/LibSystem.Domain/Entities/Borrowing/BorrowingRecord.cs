using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities.Borrowing
{
    public sealed class BorrowingRecord : BaseEntity, IAggregateRoot
    {
        private BookId bookId;
        private MemberId memberId;
        private DateTime borrowedAt;
        private DateTime? returnedAt;

        public BorrowingId BorrowingId => Id > 0 ? BorrowingId.Create(Id) : BorrowingId.CreateNew();

        public BookId BookId
        {
            get => bookId;
            private set => bookId = value ?? throw new ArgumentNullException(nameof(BookId));
        }

        public MemberId MemberId
        {
            get => memberId;
            private set => memberId = value ?? throw new ArgumentNullException(nameof(MemberId));
        }

        public DateTime BorrowedAt
        {
            get => borrowedAt;
            private set => borrowedAt = value;
        }

        public DateTime? ReturnedAt
        {
            get => returnedAt;
            private set => returnedAt = value;
        }

        // Computed properties for business logic
        public bool IsActive => !ReturnedAt.HasValue;
        public TimeSpan? BorrowDuration => ReturnedAt?.Subtract(BorrowedAt);
        public int DaysBorrowed => IsActive ?
            (int)(DateTime.UtcNow - BorrowedAt).TotalDays :
            (int)(ReturnedAt!.Value - BorrowedAt).TotalDays;

        public bool IsOverdue()
        {
            const int maxBorrowDays = 14;
            return DaysBorrowed > maxBorrowDays;
        }

        // Private constructor for EF Core
        private BorrowingRecord() { }

        //Factory method to create a new borrowing record
        public static BorrowingRecord Create(BookId bookId, MemberId memberId)
        {
            return new BorrowingRecord
            {
                // No need to set BorrowingId - it will be computed from Id
                BookId = bookId,
                MemberId = memberId,
                BorrowedAt = DateTime.UtcNow
            };
        }

        /// Factory method for reconstructing from persistence
        public static BorrowingRecord Restore(int id, int bookId, int memberId,
            DateTime borrowedAt, DateTime? returnedAt)
        {
            return new BorrowingRecord
            {
                Id = id,
                // No need to set BorrowingId - it will be computed from Id
                BookId = BookId.Create(bookId),
                MemberId = MemberId.Create(memberId),
                BorrowedAt = borrowedAt,
                ReturnedAt = returnedAt
            };
        }

        public void MarkAsReturned()
        {
            if (ReturnedAt.HasValue)
                throw new InvalidOperationException("Book has already been returned.");

            ReturnedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            var status = IsActive ? $"Active ({DaysBorrowed} days)" : $"Returned after {DaysBorrowed} days";
            return $"Borrowing {BorrowingId.Value}: Book {BookId.Value} by Member {MemberId.Value} - {status}";
        }
    }
}
