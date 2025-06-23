using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Repositories
{
    public interface IBookRepository
    {
        // Basic CRUD operations
        Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default);
        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Book book, CancellationToken cancellationToken = default);
        void Update(Book book);
        void Remove(Book book);

        // Domain-specific queries
        Task<IReadOnlyList<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Book>> GetBooksByCategoryAsync(Book.BookCategory category, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Book>> GetBooksByAuthorAsync(string author, CancellationToken cancellationToken = default);
        Task<Book?> GetByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default);
    }
}
