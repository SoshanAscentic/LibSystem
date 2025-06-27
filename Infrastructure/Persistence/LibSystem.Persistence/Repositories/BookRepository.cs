using LibSystem.Application.Repositories;
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

            return await dbSet.FirstOrDefaultAsync(b => b.BookId.Value == id.Value, cancellationToken);
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

            return await dbSet
                .FirstOrDefaultAsync(b => b.Title == title && b.PublicationYear.Value == publicationYear, cancellationToken);
        }

        public async Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default)
        {
            if (id == null) return false;

            return await dbSet.AnyAsync(b => b.BookId.Value == id.Value, cancellationToken);
        }
            public async Task<bool> ExistsByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            return await dbSet.AnyAsync(b => b.Title == title && b.PublicationYear.Value == publicationYear, cancellationToken);
        }

        

        public override async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author)
                .ThenBy(b => b.PublicationYear.Value)
                .ToListAsync(cancellationToken);
        }

        public override async Task AddAsync(Book entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Additional validation: Check for duplicates before adding
            var existingBook = await GetByTitleAndYearAsync(entity.Title, entity.PublicationYear.Value, cancellationToken);
            if (existingBook != null)
            {
                throw new InvalidOperationException($"A book with title '{entity.Title}' and publication year {entity.PublicationYear.Value} already exists.");
            }

            await base.AddAsync(entity, cancellationToken);
        }

    }
}
