using LibSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.ValueObjects
{
    public sealed class MemberId : ValueObject
    {
        public int Value { get; private set; }

        private MemberId(int value)
        {
            if (value < 0) // Changed: Allow 0 for new entities
                throw new ArgumentOutOfRangeException(nameof(value), "Member ID cannot be negative.");
            Value = value;
        }

        public static MemberId Create(int value)
        {
            if (value <= 0)
                throw new ArgumentException("MemberId must be positive", nameof(value));
            return new MemberId(value);
        }

        public static MemberId CreateNew() => new(0); // For new entities before persistence

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator int(MemberId memberId) => memberId.Value;
        public static explicit operator MemberId(int value) => Create(value);

        public override string ToString() => Value.ToString();
    }
}
