using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Exceptions
{
    public sealed class DuplicateBookException : DomainException
    {
        public string Title { get; }
        public int PublicationYear { get; }

        public DuplicateBookException(string title, int publicationYear)
            : base($"A book with title '{title}' and publication year {publicationYear} already exists.")
        {
            Title = title;
            PublicationYear = publicationYear;
        }
    }
}
