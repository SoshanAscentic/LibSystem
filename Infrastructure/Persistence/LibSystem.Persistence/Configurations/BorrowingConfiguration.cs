using LibSystem.Domain.Entities.Borrowing;
using LibSystem.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence.Configurations
{
    public class BorrowingConfiguration : IEntityTypeConfiguration<BorrowingRecord>
    {
        public void Configure(EntityTypeBuilder<BorrowingRecord> builder)
        {
            builder.ToTable("BorrowingRecords");

            // PRIMARY KEY: Use base entity Id (hidden from domain)
            builder.HasKey(br => br.Id);
            builder.Property(br => br.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");

            // DOMAIN ID: Map to separate column, sync with base Id
            builder.Property(br => br.BorrowingId)
                .HasConversion(
                    borrowingId => borrowingId.Value,
                    value => value > 0 ? BorrowingId.Create(value) : BorrowingId.CreateNew())
                .HasColumnName("BorrowingId") // ✅ Separate column
                .ValueGeneratedNever();

            // Foreign Keys - reference the domain ID columns
            builder.Property(br => br.BookId)
                .HasConversion(
                    bookId => bookId.Value,
                    value => BookId.Create(value))
                .HasColumnName("BookId")
                .IsRequired();

            builder.Property(br => br.MemberId)
                .HasConversion(
                    memberId => memberId.Value,
                    value => MemberId.Create(value))
                .HasColumnName("MemberId")
                .IsRequired();

            // Timestamps
            builder.Property(br => br.BorrowedAt)
                .HasColumnName("BorrowedAt")
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(br => br.ReturnedAt)
                .HasColumnName("ReturnedAt")
                .HasColumnType("datetime2")
                .IsRequired(false);

            // Ignore computed properties
            builder.Ignore(br => br.IsActive);
            builder.Ignore(br => br.BorrowDuration);
            builder.Ignore(br => br.DaysBorrowed);

            // Indexes
            builder.HasIndex(br => br.BorrowingId)
                .IsUnique()
                .HasDatabaseName("IX_BorrowingRecords_BorrowingId");

            builder.HasIndex(br => br.BookId)
                .HasDatabaseName("IX_BorrowingRecords_BookId");

            builder.HasIndex(br => br.MemberId)
                .HasDatabaseName("IX_BorrowingRecords_MemberId");

            builder.HasIndex(br => br.BorrowedAt)
                .HasDatabaseName("IX_BorrowingRecords_BorrowedAt");

            builder.HasIndex(br => br.ReturnedAt)
                .HasDatabaseName("IX_BorrowingRecords_ReturnedAt");

            // Unique constraint for active borrowings
            builder.HasIndex(br => new { br.BookId, br.MemberId })
                .IsUnique()
                .HasFilter("[ReturnedAt] IS NULL")
                .HasDatabaseName("UQ_BorrowingRecords_Active");

            // Check constraints
            builder.ToTable("BorrowingRecords", tb =>
            {
                tb.HasCheckConstraint("CK_BorrowingRecords_BorrowedAt",
                    "[BorrowedAt] <= GETUTCDATE()");

                tb.HasCheckConstraint("CK_BorrowingRecords_ReturnedAt",
                    "[ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt]");
            });

            // Audit fields
            builder.Property(br => br.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(br => br.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Ignore(br => br.DomainEvents);
        }
    }
}
