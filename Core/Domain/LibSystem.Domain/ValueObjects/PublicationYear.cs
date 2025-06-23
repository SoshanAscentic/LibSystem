using LibSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.ValueObjects
{
    public sealed class PublicationYear : ValueObject
    {
        public int Value { get; private set; }
        private PublicationYear(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Publication year cannot be negative.");
            Value = value;
        }
        public static PublicationYear Create(int value)
        {
            const int MinYear = 1450; // Gutenberg printing press era (The reason why 1450 is chosen)
            var currentYear = DateTime.Now.Year;

            if (value < MinYear || value > currentYear)
                throw new ArgumentException(
                    $"Publication year must be between {MinYear} and {currentYear}",
                    nameof(value));

            return new PublicationYear(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator int(PublicationYear year) => year.Value;
        public static explicit operator PublicationYear(int value) => Create(value);

        public override string ToString() => Value.ToString();
    }
}
