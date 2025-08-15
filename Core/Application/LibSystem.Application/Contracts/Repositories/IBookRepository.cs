// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBookRepository.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LibSystem.Domain.Common;
    using LibSystem.Domain.ValueObjects;

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

        // Added method for ID synchronization
        Task<int> GetNextBookIdAsync(CancellationToken cancellationToken = default);
    }
}
