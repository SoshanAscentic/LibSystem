using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Events
{
    public sealed class BookCreatedEvent : IDomainEvent
    {
        public BookId BookId { get; }
        public string Title { get; }
        public string Author { get; }
        public string Category { get; }
        public DateTime OccurredOn { get; }

        public BookCreatedEvent(BookId bookId, string title, string author, string category)
        {
            BookId = bookId;
            Title = title;
            Author = author;
            Category = category;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
