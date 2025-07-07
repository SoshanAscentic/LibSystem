using LibSystem.Application.Contracts.Repositories;
using LibSystem.Domain.ValueObjects;
using LibSystem.Persistence.Context;
using LibSystem.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context) : base(context)
    {
    }

    public async Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));

        // Use the Id value for querying since BookId.Value returns the database Id
        return await dbSet.FindAsync(new object[] { id.Value }, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .Where(b => b.IsAvailable)
            .OrderBy(b => b.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetBooksByCategoryAsync(Book.BookCategory category, CancellationToken cancellationToken = default)
    {
        return await dbSet
            .Where(b => b.Category == category)
            .OrderBy(b => b.Title)
            .ThenBy(b => b.Author)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetBooksByAuthorAsync(string author, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(author))
            return new List<Book>();

        return await dbSet
            .Where(b => b.Author.Contains(author))
            .OrderBy(b => b.Author)
            .ThenBy(b => b.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        // Get all books with matching title, then filter by publication year on client side
        var books = await dbSet
            .Where(b => b.Title == title)
            .ToListAsync(cancellationToken);

        // Filter by publication year using domain logic (client-side)
        return books.FirstOrDefault(b => b.PublicationYear.Value == publicationYear);
    }

    public async Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default)
    {
        if (id == null) return false;

        return await dbSet.AnyAsync(b => b.Id == id.Value, cancellationToken);
    }

    public async Task<bool> ExistsByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        // Get all books with matching title, then check publication year on client side
        var books = await dbSet
            .Where(b => b.Title == title)
            .ToListAsync(cancellationToken);

        return books.Any(b => b.PublicationYear.Value == publicationYear);
    }

    public override async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Get all books and sort on client side
        var books = await dbSet
            .OrderBy(b => b.Title)
            .ThenBy(b => b.Author)
            .ToListAsync(cancellationToken);

        // Additional sorting by publication year on client side if needed
        return books.OrderBy(b => b.PublicationYear.Value).ToList();
    }

    public async Task<int> GetNextBookIdAsync(CancellationToken cancellationToken = default)
    {
        // This method is no longer needed since BookId is computed from database Id
        // But keeping it for interface compatibility
        var lastBook = await dbSet
            .OrderByDescending(b => b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return lastBook?.Id + 1 ?? 1;
    }

    // SOLUTION 1: Simplified - No more BookId management needed
    public override async Task AddAsync(Book entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // Simply add the entity - BookId will be computed from the database Id after save
        await base.AddAsync(entity, cancellationToken);
    }

    public override void Update(Book entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // No BookId synchronization needed - it's computed automatically
        base.Update(entity);
    }
}
