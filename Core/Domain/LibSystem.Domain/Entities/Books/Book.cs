using LibSystem.Domain.Common;
using LibSystem.Domain.Events;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.ValueObjects;

public class Book : BaseEntity, IAggregateRoot
{
    private string title;
    private string author;
    private PublicationYear publicationYear;
    private BookCategory category;
    private bool isAvailable;

    // SOLUTION 1: BookId is now a computed property that always returns the database Id
    public BookId BookId => Id > 0 ? BookId.Create(Id) : BookId.CreateNew();

    public enum BookCategory
    {
        Fiction = 0,
        History = 1,
        Child = 2
    }

    public string Title
    {
        get => title;
        private set => title = ValidateTitle(value);
    }

    public string Author
    {
        get => author;
        private set => author = ValidateAuthor(value);
    }

    public PublicationYear PublicationYear
    {
        get => publicationYear;
        private set => publicationYear = value ?? throw new ArgumentNullException(nameof(value), "Publication year cannot be null.");
    }

    public BookCategory Category
    {
        get => category;
        private set => category = value;
    }

    public bool IsAvailable
    {
        get => isAvailable;
        private set => isAvailable = value;
    }

    //Private constructor for EF Core
    private Book() { }

    // Factory method for creating new books (Domain-driven approach)
    public static Book Create(string title, string author, int publicationYear, BookCategory category)
    {
        var book = new Book
        {
            // No need to set BookId - it will be computed from Id
            Title = ValidateTitle(title),
            Author = ValidateAuthor(author),
            PublicationYear = PublicationYear.Create(publicationYear),
            Category = category,
            IsAvailable = true
        };

        // Raise domain event for book creation
        // Note: BookId will be available after the entity is saved and has an Id
        book.AddDomainEvent(new BookCreatedEvent(
            book.BookId, // This will be computed after save
            book.Title,
            book.Author,
            book.Category.ToString()));

        return book;
    }

    // Factory method for reconstructing from persistence (used by EF Core)
    public static Book Restore(int id, string title, string author, int publicationYear,
        BookCategory category, bool isAvailable)
    {
        return new Book
        {
            Id = id,
            // No need to set BookId - it will be computed from Id
            title = title,
            author = author,
            publicationYear = PublicationYear.Create(publicationYear),
            category = category,
            isAvailable = isAvailable
        };
    }

    public void Borrow(MemberId memberId)
    {
        if (!IsAvailable)
            throw InvalidBorrowingException.BookNotAvailable(BookId.Value);

        IsAvailable = false;
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new BookBorrowedEvent(BookId, memberId, DateTime.UtcNow));
    }

    public void Return(MemberId memberId, DateTime borrowedAt)
    {
        if (IsAvailable)
            throw InvalidBorrowingException.BookAlreadyReturned(BookId.Value);

        IsAvailable = true;
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new BookReturnedEvent(BookId, memberId, DateTime.UtcNow, borrowedAt));
    }

    public void UpdateDetails(string title, string author, int publicationYear, BookCategory category)
    {
        Title = title;
        Author = author;
        PublicationYear = PublicationYear.Create(publicationYear);
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool BelongsToCategory(BookCategory category)
    {
        return Category == category;
    }

    public bool MatchesAuthor(string authorSearchTerm)
    {
        if (string.IsNullOrWhiteSpace(authorSearchTerm))
            return false;

        return Author.Contains(authorSearchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));

        return trimmedTitle;
    }

    private static string ValidateAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be null or empty.", nameof(author));

        var trimmedAuthor = author.Trim();
        if (trimmedAuthor.Length > 100)
            throw new ArgumentException("Author cannot exceed 100 characters.", nameof(author));

        return trimmedAuthor;
    }

    public override string ToString()
    {
        return $"Book: {Title} by {Author} ({PublicationYear.Value}) - {Category} - Available: {IsAvailable}";
    }
}