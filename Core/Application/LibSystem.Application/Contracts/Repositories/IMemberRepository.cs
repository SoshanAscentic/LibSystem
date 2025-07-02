// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMemberRepository.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.Repositories
{
    using LibSystem.Domain.Common;
    using LibSystem.Domain.Entities.Members;
    using LibSystem.Domain.ValueObjects;

    public interface IMemberRepository : IGenericRepository<Member>
    {
        // Member-specific operations
        Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Member>> GetMembersWithBorrowedBooksAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetMembersByTypeAsync<T>(CancellationToken cancellationToken = default) where T : Member;

        Task<bool> ExistsAsync(MemberId id, CancellationToken cancellationToken = default);

        Task<int> GetNextMemberIdAsync(CancellationToken cancellationToken = default);
    }
}
