using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Exceptions
{
    public sealed class BookNotFoundException : DomainException
    {
        public BookId BookId { get; }

        public BookNotFoundException(BookId bookId)
            : base($"Book with ID {bookId.Value} was not found.")
        {
            BookId = bookId;
        }

        public BookNotFoundException(int bookId)
            : this(BookId.Create(bookId))
        {
        }
    }
}
