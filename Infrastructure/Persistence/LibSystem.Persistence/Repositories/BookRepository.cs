using LibSystem.Application.Contracts.Repositories;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.ValueObjects;
using LibSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            // Use base Id for querying
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

        // Avoid value object property access in database queries
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
            var lastBook = await dbSet
                .OrderByDescending(b => b.Id) // Use base Id for ordering
                .FirstOrDefaultAsync(cancellationToken);

            return lastBook?.Id + 1 ?? 1;
        }

        public override async Task AddAsync(Book entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Get next base Id and set it as the BookId before adding
            int nextId = await GetNextBookIdAsync(cancellationToken);

            // Set the BookId using reflection to ensure it syncs with the database Id
            var bookIdProperty = entity.GetType().GetProperty("BookId");
            bookIdProperty?.SetValue(entity, BookId.Create(nextId));

            await base.AddAsync(entity, cancellationToken);
        }

        public override void Update(Book entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Ensure BookId is synced with database Id during updates
            if (entity.Id > 0 && entity.BookId.Value != entity.Id)
            {
                var bookIdProperty = entity.GetType().GetProperty("BookId");
                bookIdProperty?.SetValue(entity, BookId.Create(entity.Id));
            }

            base.Update(entity);
        }
    }
}
