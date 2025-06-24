using LibSystem.Domain.Common;
using LibSystem.Domain.Entities.Members;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Repositories
{
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
