using LibSystem.Domain.Entities.Books;
using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.Entities.Members;
using LibSystem.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence.Context
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        //Books aggregate root - represents the books available in the library
        public DbSet<Book> Books { get; set; } = null!;

        // Members aggregate root - represents all library members (Regular, Staff types)
        public DbSet<Member> Members { get; set; } = null!;

        // Borrowing aggregate root - represents the borrowing transactions of books by members
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; } = null!;

        // Configures entity mappings and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
        }

        // Configures database provider options
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Enable sensitive data logging in development
            if (System.Diagnostics.Debugger.IsAttached)
            {
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update audit fields before saving
            UpdateAuditFields();

            // Process domain events (will be handled by repositories)
            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        // Synchronous version of SaveChanges with audit field updates
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Domain.Common.BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (Domain.Common.BaseEntity)entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
