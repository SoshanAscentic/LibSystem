using LibSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.ValueObjects
{
    public sealed class Name : ValueObject
    {
        public string Value { get; private set; }

        private Name (string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or empty.", nameof(value));
            Value = value.Trim();
        }

        public static Name Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or empty", nameof(value));

            if (value.Length > 100)
                throw new ArgumentException("Name cannot exceed 100 characters", nameof(value));

            var trimmedValue = value.Trim();
            if (trimmedValue.Length < 1)
                throw new ArgumentException("Name cannot be empty after trimming", nameof(value));

            return new Name(trimmedValue);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(Name name) => name.Value;
        public static explicit operator Name(string value) => Create(value);

        public override string ToString() => Value;
    }
}
