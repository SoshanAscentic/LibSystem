using LibSystem.Application.Repositories;
using LibSystem.Domain.Entities.Members;
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
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken = default)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return await dbSet.FirstOrDefaultAsync(m => m.MemberId.Value == id.Value, cancellationToken);
        }

        public async Task<IReadOnlyList<Member>> GetMembersWithBorrowedBooksAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet
                .Where(m => m.BorrowedBooksCount > 0)
                .OrderBy(m => m.Name.Value)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> GetMembersByTypeAsync<T>(CancellationToken cancellationToken = default) where T : Member
        {
            return await dbSet
                .OfType<T>()
                .OrderBy(m => m.Name.Value)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(MemberId id, CancellationToken cancellationToken = default)
        {
            if (id == null) return false;

            return await dbSet.AnyAsync(m => m.MemberId.Value == id.Value, cancellationToken);
        }

        public async Task<int> GetNextMemberIdAsync(CancellationToken cancellationToken = default)
        {
            var lastMember = await dbSet
                .OrderByDescending(m => m.MemberId.Value)
                .FirstOrDefaultAsync(cancellationToken);

            return lastMember?.MemberId.Value + 1 ?? 1;
        }

        public override async Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet
                .OrderBy(m => m.Name.Value)
                .ThenBy(m => m.MemberId.Value)
                .ToListAsync(cancellationToken);
        }

        public override async Task AddAsync(Member entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Validate that the member doesn't already exist (defensive programming)
            if (entity.MemberId.Value > 0)
            {
                var existingMember = await GetByIdAsync(entity.MemberId, cancellationToken);
                if (existingMember != null)
                {
                    throw new InvalidOperationException($"Member with ID {entity.MemberId.Value} already exists.");
                }
            }

            await base.AddAsync(entity, cancellationToken);
        }

        public override void Update(Member entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // Validate business rules before updating
            if (entity.BorrowedBooksCount < 0)
            {
                throw new InvalidOperationException("Member cannot have negative borrowed books count.");
            }

            if (entity.BorrowedBooksCount > Member.MAX_BORROWED_BOOKS)
            {
                throw new InvalidOperationException($"Member cannot have more than {Member.MAX_BORROWED_BOOKS} borrowed books.");
            }

            base.Update(entity);
        }
    }

}
}
