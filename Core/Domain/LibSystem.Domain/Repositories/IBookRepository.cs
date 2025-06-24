using LibSystem.Domain.Common;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Repositories
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        // Book-specific operations
        Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Book>> GetBooksByCategoryAsync(Book.BookCategory category, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Book>> GetBooksByAuthorAsync(string author, CancellationToken cancellationToken = default);
        Task<Book?> GetByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByTitleAndYearAsync(string title, int publicationYear, CancellationToken cancellationToken = default);
    }
}
