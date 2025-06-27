using LibSystem.Domain.Entities.Members;
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
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("Members");

            // Configure primary key using the base entity Id
            builder.HasKey(m => m.Id);

            // Configure MemberId value object mapping
            builder.Property(m => m.MemberId)
                .HasConversion(
                    memberId => memberId.Value,
                    value => MemberId.Create(value))
                .HasColumnName("MemberId")
                .ValueGeneratedNever(); // Changed: Don't auto-generate MemberId

            // Name value object configuration
            builder.Property(m => m.Name)
                .HasConversion(
                    name => name.Value,
                    value => Domain.ValueObjects.Name.Create(value))
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();

            // BorrowedBooksCount property
            builder.Property(m => m.BorrowedBooksCount)
                .HasColumnName("BorrowedBooksCount")
                .HasDefaultValue(0)
                .IsRequired();

            // Configure discriminator for inheritance hierarchy
            builder.HasDiscriminator<string>("MemberType")
                .HasValue<RegularMember>("RegularMember")
                .HasValue<MinorStaff>("MinorStaff")
                .HasValue<ManagementStaff>("ManagementStaff");

            // Configure discriminator column
            builder.Property("MemberType")
                .HasMaxLength(20)
                .IsRequired();

            // Index on Name for name-based searches
            builder.HasIndex(m => m.Name)
                .HasDatabaseName("IX_Members_Name");

            // Index on MemberType for type-based filtering
            builder.HasIndex("MemberType")
                .HasDatabaseName("IX_Members_MemberType");

            // Index on BorrowedBooksCount for reporting
            builder.HasIndex(m => m.BorrowedBooksCount)
                .HasDatabaseName("IX_Members_BorrowedBooksCount");



            /*// Add check constraint for borrowed books count
            builder.HasCheckConstraint("CK_Members_BorrowedBooksCount",
                $"[BorrowedBooksCount] >= 0 AND [BorrowedBooksCount] <= {Member.MAX_BORROWED_BOOKS}");

            // Add check constraint for valid member types
            builder.HasCheckConstraint("CK_Members_MemberType",
                "[MemberType] IN ('RegularMember', 'MinorStaff', 'ManagementStaff')");*/

            builder.ToTable("Members", tb =>
            {
                tb.HasCheckConstraint("CK_Members_BorrowedBooksCount",
                    $"[BorrowedBooksCount] >= 0 AND [BorrowedBooksCount] <= {Member.MAX_BORROWED_BOOKS}");

                tb.HasCheckConstraint("CK_Members_MemberType",
                    "[MemberType] IN ('RegularMember', 'MinorStaff', 'ManagementStaff')");
            });

            // Configure audit fields from BaseEntity
            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(m => m.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore domain events collection as it's not persisted
            builder.Ignore(m => m.DomainEvents);
        }
    }
}
