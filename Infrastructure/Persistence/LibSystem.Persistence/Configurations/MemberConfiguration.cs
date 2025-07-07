using LibSystem.Domain.Entities.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        // PRIMARY KEY: Use base entity Id
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id");

        // MemberId will be a computed property that always returns the database Id
        builder.Ignore(m => m.MemberId);

        // Name value object
        builder.Property(m => m.Name)
            .HasConversion(
                name => name.Value,
                value => LibSystem.Domain.ValueObjects.Name.Create(value))
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        // BorrowedBooksCount
        builder.Property(m => m.BorrowedBooksCount)
            .HasColumnName("BorrowedBooksCount")
            .HasDefaultValue(0)
            .IsRequired();

        // Configure inheritance
        builder.HasDiscriminator<string>("MemberType")
            .HasValue<RegularMember>("RegularMember")
            .HasValue<MinorStaff>("MinorStaff")
            .HasValue<ManagementStaff>("ManagementStaff");

        builder.Property("MemberType")
            .HasMaxLength(20)
            .IsRequired();

        // Indexes
        builder.HasIndex(m => m.Name)
            .HasDatabaseName("IX_Members_Name");

        builder.HasIndex("MemberType")
            .HasDatabaseName("IX_Members_MemberType");

        builder.HasIndex(m => m.BorrowedBooksCount)
            .HasDatabaseName("IX_Members_BorrowedBooksCount");

        // Check constraints
        builder.ToTable("Members", tb =>
        {
            tb.HasCheckConstraint("CK_Members_BorrowedBooksCount",
                $"[BorrowedBooksCount] >= 0 AND [BorrowedBooksCount] <= {Member.MAX_BORROWED_BOOKS}");

            tb.HasCheckConstraint("CK_Members_MemberType",
                "[MemberType] IN ('RegularMember', 'MinorStaff', 'ManagementStaff')");
        });

        // Audit fields
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Ignore(m => m.DomainEvents);
    }
}