using LibSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.ValueObjects
{
    public sealed class BookId : ValueObject
    {
        public int Value { get; private set; }

        public BookId(int value)
        {
            if (value < 0) // Changed: Allow 0 for new entities
                throw new ArgumentOutOfRangeException(nameof(value), "Book ID cannot be negative.");
            Value = value;
        }

        public static BookId Create(int value)
        {
            if (value <= 0)
                throw new ArgumentException("BookId must be positive", nameof(value));

            return new BookId(value);
        }

        public static BookId CreateNew() => new(0); //For new entities before persistence

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator int(BookId bookId) => bookId.Value;
        public static explicit operator BookId(int value) => Create(value);

        public override string ToString() => Value.ToString();
    }
}
