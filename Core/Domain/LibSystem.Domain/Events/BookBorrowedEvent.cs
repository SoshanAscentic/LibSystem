using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Events
{
    public sealed class BookBorrowedEvent : IDomainEvent
    {
        public BookId BookId { get; }
        public MemberId MemberId { get; }
        public DateTime BorrowedAt { get; }
        public DateTime OccurredOn { get; }

        public BookBorrowedEvent(BookId bookId, MemberId memberId, DateTime borrowedAt)
        {
            BookId = bookId;
            MemberId = memberId;
            BorrowedAt = borrowedAt;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
