using LibSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.ValueObjects
{
    public sealed class BorrowingId : ValueObject
    {
        public int Value { get; private set; }
        private BorrowingId(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Borrowing ID must be a positive integer.");
            Value = value;
        }
        public static BorrowingId Create(int value)
        {
            if (value <= 0)
                throw new ArgumentException("BorrowingId must be positive", nameof(value));
            return new BorrowingId(value);
        }
        public static BorrowingId CreateNew() => new(0); // For new entities before persistence
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
        public static implicit operator int(BorrowingId borrowingId) => borrowingId.Value;
        public static explicit operator BorrowingId(int value) => Create(value);
        public override string ToString() => Value.ToString();
    }
}
