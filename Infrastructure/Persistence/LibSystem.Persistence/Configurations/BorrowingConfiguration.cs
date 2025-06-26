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

            // Configrution primary key using the base entity Id
            builder.HasKey(br => br.Id);

            //Configure BorrowingId value object mapping
            builder.Property(br => br.BorrowingId)
                .HasConversion(
                    borrowingId => borrowingId.Value,              
                    value => BorrowingId.Create(value)) //Converting from int to BorrowingId value object
                .HasColumnName("BorrowingId")
                .ValueGeneratedOnAdd();

            // Configure BookId value object as foreign key
            builder.Property(br => br.BookId)
                .HasConversion(
                    bookId => bookId.Value,               
                    value => BookId.Create(value))        
                .HasColumnName("BookId")
                .IsRequired();

            // Configure MemberId value object as foreign key
            builder.Property(br => br.MemberId)
                .HasConversion(
                    memberId => memberId.Value,           
                    value => MemberId.Create(value))
                .HasColumnName("MemberId")
                .IsRequired();


            // Configure BorrowedAt timestamp
            builder.Property(br => br.BorrowedAt)
                .HasColumnName("BorrowedAt")
                .HasColumnType("datetime2")
                .IsRequired();

            // Configure ReturnedAt nullable timestamp
            builder.Property(br => br.ReturnedAt)
                .HasColumnName("ReturnedAt")
                .HasColumnType("datetime2")
                .IsRequired(false);


            // These are computed properties that don't need database storage
            builder.Ignore(br => br.IsActive);
            builder.Ignore(br => br.BorrowDuration);
            builder.Ignore(br => br.DaysBorrowed);

            builder.HasIndex(br => br.BookId)
                .HasDatabaseName("IX_BorrowingRecords_BookId");

            builder.HasIndex(br => br.MemberId)
                .HasDatabaseName("IX_BorrowingRecords_MemberId");

            builder.HasIndex(br => br.BorrowedAt)
                .HasDatabaseName("IX_BorrowingRecords_BorrowedAt");

            builder.HasIndex(br => br.ReturnedAt)
                .HasDatabaseName("IX_BorrowingRecords_ReturnedAt");

            builder.HasIndex(br => new { br.BookId, br.MemberId, br.ReturnedAt })
                .HasDatabaseName("IX_BorrowingRecords_Active");

            builder.HasIndex(br => new { br.BorrowedAt, br.ReturnedAt })
                .HasDatabaseName("IX_BorrowingRecords_Duration");


            /*// Ensure BorrowedAt is not in the future
            builder.HasCheckConstraint("CK_BorrowingRecords_BorrowedAt",
                "[BorrowedAt] <= GETUTCDATE()");

            // Ensure ReturnedAt is not before BorrowedAt (when not null)
            builder.HasCheckConstraint("CK_BorrowingRecords_ReturnedAt",
                "[ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt]");*/

            builder.ToTable("BorrowingRecords", tb =>
            {
                tb.HasCheckConstraint("CK_BorrowingRecords_BorrowedAt",
                    "[BorrowedAt] <= GETUTCDATE()");

                tb.HasCheckConstraint("CK_BorrowingRecords_ReturnedAt",
                    "[ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt]");
            });



            // Unique constraint to prevent duplicate active borrowings
            // (same book cannot be borrowed by same member multiple times simultaneously)
            builder.HasIndex(br => new { br.BookId, br.MemberId })
                .IsUnique()
                .HasFilter("[ReturnedAt] IS NULL")  // Only apply to active borrowings
                .HasDatabaseName("UQ_BorrowingRecords_Active");

            // Configure audit fields from BaseEntity
            builder.Property(br => br.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(br => br.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");


            // Ignore domain events collection as it's not persisted
            builder.Ignore(br => br.DomainEvents);



        }
    }
}
