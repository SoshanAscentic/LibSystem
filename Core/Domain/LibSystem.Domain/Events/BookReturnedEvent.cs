using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Events
{
    public sealed class BookReturnedEvent : IDomainEvent
    {
        public BookId BookId { get; }
        public MemberId MemberId { get; }
        public DateTime ReturnedAt { get; }
        public DateTime BorrowedAt { get; }
        public TimeSpan BorrowDuration => ReturnedAt - BorrowedAt;
        public DateTime OccurredOn { get; }

        public BookReturnedEvent(BookId bookId, MemberId memberId, DateTime returnedAt, DateTime borrowedAt)
        {
            BookId = bookId;
            MemberId = memberId;
            ReturnedAt = returnedAt;
            BorrowedAt = borrowedAt;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
