using LibSystem.Domain.Common;
using LibSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IAggregateRoot
    {
        protected readonly LibraryDbContext context;
        protected readonly DbSet<T> dbSet;

        public GenericRepository(LibraryDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            dbSet = context.Set<T>();
        }

        // Query operations
        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbSet.CountAsync(predicate, cancellationToken);
        }

        // Command operations
        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            await dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            dbSet.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            dbSet.UpdateRange(entities);
        }

        public virtual void Remove(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            dbSet.RemoveRange(entities);
        }
    }
}
