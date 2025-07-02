using LibSystem.Application.Contracts.UoW;
using LibSystem.Domain.Common;
using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.Entities.Members;
using LibSystem.Domain.ValueObjects;
using LibSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence.UoW
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly LibraryDbContext context;
        private bool disposed = false;

        public UnitOfWork(LibraryDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool HasActiveTransaction => context.Database.CurrentTransaction != null;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set audit fields before saving
            SetAuditFields();

            // Process domain events before saving
            await ProcessDomainEventsAsync(cancellationToken);

            // Get entities that need ID sync before save
            var entitiesToSync = GetEntitiesNeedingIdSync();

            // Save all changes to database (this generates the database IDs)
            var result = await context.SaveChangesAsync(cancellationToken);

            // Sync domain IDs after successful save
            SyncDomainIdsAfterSave(entitiesToSync);

            return result;
        }

        public async Task BeginTransactionAsync()
        {
            if (HasActiveTransaction)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            // Use execution strategy to handle retries properly with transactions
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await context.Database.BeginTransactionAsync();
            });
        }

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

        public async Task RollbackTransactionAsync()
        {
            if (!HasActiveTransaction)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            await context.Database.RollbackTransactionAsync();
        }

        // Executes the operation within a transaction using the execution strategy
        // This is the recommended way to handle complex operations that need transactions with retry logic
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await operation();
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        // Executes the operation within a transaction using the execution strategy (void version)
        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await operation();
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        private void SetAuditFields()
        {
            var entries = context.ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }

        private List<BaseEntity> GetEntitiesNeedingIdSync()
        {
            return context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .Where(entity =>
                    (entity is Book book && book.BookId.Value == 0) ||
                    (entity is Member member && member.MemberId.Value == 0) ||
                    (entity is BorrowingRecord borrowing && borrowing.BorrowingId.Value == 0))
                .ToList();
        }

        private void SyncDomainIdsAfterSave(List<BaseEntity> entitiesToSync)
        {
            foreach (var entity in entitiesToSync)
            {
                if (entity.Id > 0) // Entity now has database-generated ID
                {
                    var databaseId = entity.Id;

                    try
                    {
                        // Sync Book entities
                        if (entity is Book book)
                        {
                            SyncBookId(book, databaseId);
                        }
                        // Sync Member entities
                        else if (entity is Member member)
                        {
                            SyncMemberId(member, databaseId);
                        }
                        // Sync BorrowingRecord entities
                        else if (entity is BorrowingRecord borrowing)
                        {
                            SyncBorrowingId(borrowing, databaseId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to sync ID for {entity.GetType().Name}: {ex.Message}");
                        throw; // Re-throw to ensure data consistency
                    }
                }
            }
        }

        private static void SyncBookId(Book book, int databaseId)
        {
            var bookIdProperty = typeof(Book).GetProperty("BookId");
            if (bookIdProperty != null && bookIdProperty.CanWrite)
            {
                var newBookId = BookId.Create(databaseId);
                bookIdProperty.SetValue(book, newBookId);
                Console.WriteLine($"✅ Synced Book ID: {databaseId}");
            }
            else
            {
                throw new InvalidOperationException("Cannot sync BookId property - property not found or not writable");
            }
        }

        private static void SyncMemberId(Member member, int databaseId)
        {
            var memberIdProperty = typeof(Member).GetProperty("MemberId");
            if (memberIdProperty != null && memberIdProperty.CanWrite)
            {
                var newMemberId = MemberId.Create(databaseId);
                memberIdProperty.SetValue(member, newMemberId);
                Console.WriteLine($"✅ Synced Member ID: {databaseId}");
            }
            else
            {
                throw new InvalidOperationException("Cannot sync MemberId property - property not found or not writable");
            }
        }

        private static void SyncBorrowingId(BorrowingRecord borrowing, int databaseId)
        {
            var borrowingIdProperty = typeof(BorrowingRecord).GetProperty("BorrowingId");
            if (borrowingIdProperty != null && borrowingIdProperty.CanWrite)
            {
                var newBorrowingId = BorrowingId.Create(databaseId);
                borrowingIdProperty.SetValue(borrowing, newBorrowingId);
                Console.WriteLine($"✅ Synced Borrowing ID: {databaseId}");
            }
            else
            {
                throw new InvalidOperationException("Cannot sync BorrowingId property - property not found or not writable");
            }
        }

        private async Task ProcessDomainEventsAsync(CancellationToken cancellationToken)
        {
            var domainEntities = context.ChangeTracker
                .Entries<IAggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            // Clear events before processing to avoid duplicate processing
            domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                Console.WriteLine($"Domain Event: {domainEvent.GetType().Name} occurred at {domainEvent.OccurredOn}");
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                context?.Dispose();
                disposed = true;
            }
        }
    }
}
