using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities
{
    public class Book
    {
        public enum BookCategory { Fiction, History, Child }

        private string title;
        private string author;
        private int publicationYear;
        public int BookId { get; set; }

        public string Title
        {
            get => title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be null or empty.", nameof(Title));
                title = value.Trim();
            }
        }

        public string Author
        {
            get => author;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Author cannot be null or empty.", nameof(Author));
                author = value.Trim();
            }
        }

        public int PublicationYear
        {
            get => publicationYear;
            set
            {
                if (value < 1450 || value > DateTime.Now.Year)
                    throw new ArgumentOutOfRangeException(nameof(PublicationYear),
                        $"Publication year must be between 1450 and {DateTime.Now.Year}.");
                publicationYear = value;
            }
        }

        public BookCategory Category { get; set; }
        public bool IsAvailable { get; set; }

        // Parameterless constructor for EF Core
        public Book() { }

        public Book(string title, string author, int publicationYear, BookCategory category)
        {
            Title = title.Trim();
            Author = author.Trim();
            PublicationYear = publicationYear;
            Category = category;
            IsAvailable = true;
        }

        public override string ToString()
        {
            return $"Title: {Title}, Author: {Author}, Year: {PublicationYear}, Category: {Category}, Available: {IsAvailable}";
        }
    }
}
