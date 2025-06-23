using LibSystem.Domain.Entities.Members;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Repositories
{
    public interface IMemberRepository
    {
        // Basic CRUD operations
        Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken = default);
        Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Member member, CancellationToken cancellationToken = default);
        void Update(Member member);
        void Remove(Member member);

        // Domain-specific queries
        Task<IReadOnlyList<Member>> GetMembersWithBorrowedBooksAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetMembersByTypeAsync<T>(CancellationToken cancellationToken = default) where T : Member;
        Task<bool> ExistsAsync(MemberId id, CancellationToken cancellationToken = default);
        Task<int> GetNextMemberIdAsync(CancellationToken cancellationToken = default);
    }
}
