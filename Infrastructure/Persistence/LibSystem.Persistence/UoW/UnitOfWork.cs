using LibSystem.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext context;

        public UnitOfWork(LibraryDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }


        // Indicates whether there's an active transaction
        public bool HasActiveTransaction => context.Database.CurrentTransaction != null;


        // Saves all changes made in the current unit of work
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        // Begins a new database transaction. Enables rollback capability for complex operations
        public async Task BeginTransactionAsync()
        {
            if (HasActiveTransaction)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            await context.Database.BeginTransactionAsync();
        }


        // Commits the current transaction. Makes all changes permanent in the database
        public async Task CommitTransactionAsync()
        {
            if (!HasActiveTransaction)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await context.Database.CommitTransactionAsync();
            }
            catch
            {
                await context.Database.RollbackTransactionAsync();
                throw;
            }
        }


        // Rolls back the current transaction. Discards all changes made since the transaction began
        public async Task RollbackTransactionAsync()
        {
            if (!HasActiveTransaction)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            await context.Database.RollbackTransactionAsync();
        }
    }
}
