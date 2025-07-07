using LibSystem.Application.Contracts.Repositories;
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

            // Use base Id for querying
            return await dbSet.FindAsync(new object[] { id.Value }, cancellationToken);
        }

        public async Task<IReadOnlyList<Member>> GetMembersWithBorrowedBooksAsync(CancellationToken cancellationToken = default)
        {
            var members = await dbSet
                .Where(m => m.BorrowedBooksCount > 0)
                .ToListAsync(cancellationToken);

            return members.OrderBy(m => m.Name.Value).ToList();
        }

        public async Task<IReadOnlyList<T>> GetMembersByTypeAsync<T>(CancellationToken cancellationToken = default) where T : Member
        {
            var members = await dbSet
                .OfType<T>()
                .ToListAsync(cancellationToken);

            return members.OrderBy(m => m.Name.Value).ToList();
        }

        public async Task<bool> ExistsAsync(MemberId id, CancellationToken cancellationToken = default)
        {
            if (id == null) return false;

            return await dbSet.AnyAsync(m => m.Id == id.Value, cancellationToken);
        }

        public async Task<int> GetNextMemberIdAsync(CancellationToken cancellationToken = default)
        {
            // This method is no longer used for pre-assignment
            // We let EF Core handle the ID assignment and sync afterward
            var lastMember = await dbSet
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return lastMember?.Id + 1 ?? 1;
        }

        public override async Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var members = await dbSet.ToListAsync(cancellationToken);

            return members
                .OrderBy(m => m.Name.Value)
                .ThenBy(m => m.Id)
                .ToList();
        }

        public override async Task AddAsync(Member entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            // FIXED: Don't pre-assign MemberId - let EF Core assign the database Id first
            // The MemberId will be synced with the database Id after SaveChanges in UnitOfWork

            // Ensure MemberId is set to default so UnitOfWork knows to sync it
            if (entity.MemberId.Value != 0)
            {
                // Reset to default so sync will happen
                var memberIdProperty = entity.GetType().GetProperty("MemberId");
                memberIdProperty?.SetValue(entity, MemberId.CreateNew());
            }

            await base.AddAsync(entity, cancellationToken);
        }

        public override void Update(Member entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity.BorrowedBooksCount < 0)
            {
                throw new InvalidOperationException("Member cannot have negative borrowed books count.");
            }

            if (entity.BorrowedBooksCount > Member.MAX_BORROWED_BOOKS)
            {
                throw new InvalidOperationException($"Member cannot have more than {Member.MAX_BORROWED_BOOKS} borrowed books.");
            }

            // FIXED: Only sync MemberId if it's different from database Id
            if (entity.Id > 0 && entity.MemberId.Value != entity.Id)
            {
                var memberIdProperty = entity.GetType().GetProperty("MemberId");
                memberIdProperty?.SetValue(entity, MemberId.Create(entity.Id));
            }

            base.Update(entity);
        }
    }
}

