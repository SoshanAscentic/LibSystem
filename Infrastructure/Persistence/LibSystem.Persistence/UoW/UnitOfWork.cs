using LibSystem.Application.Contracts.UoW;
using LibSystem.Domain.Common;
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
            // STEP 3a: Set audit fields before saving
            SetAuditFields();

            // STEP 3b: Process domain events before saving
            await ProcessDomainEventsAsync(cancellationToken);

            // STEP 3c: Save all changes in one transaction
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (HasActiveTransaction)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            await context.Database.BeginTransactionAsync();
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

            // Here you could publish domain events using MediatR
            foreach (var domainEvent in domainEvents)
            {
                Console.WriteLine($"Domain Event: {domainEvent.GetType().Name} occurred at {domainEvent.OccurredOn}");
                // In a real implementation: await mediator.Publish(domainEvent, cancellationToken);
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
