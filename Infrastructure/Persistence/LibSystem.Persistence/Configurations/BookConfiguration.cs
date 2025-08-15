using LibSystem.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        // PRIMARY KEY: Use base entity Id
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id");

        // BookId will be a computed property that always returns the database Id
        builder.Ignore(b => b.BookId);

        // Title property
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Title");

        // Author property
        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Author");

        // PublicationYear value object - store as simple int
        builder.Property(b => b.PublicationYear)
            .HasConversion(
                year => year.Value,
                value => PublicationYear.Create(value))
            .HasColumnName("PublicationYear")
            .IsRequired();

        // Category enum
        builder.Property(b => b.Category)
            .HasConversion<int>()
            .HasColumnName("Category")
            .IsRequired();

        // IsAvailable boolean
        builder.Property(b => b.IsAvailable)
            .HasColumnName("IsAvailable")
            .HasDefaultValue(true)
            .IsRequired();

        // Indexes
        builder.HasIndex(b => b.Author)
            .HasDatabaseName("IX_Books_Author");

        builder.HasIndex(b => b.Category)
            .HasDatabaseName("IX_Books_Category");

        builder.HasIndex(b => b.IsAvailable)
            .HasDatabaseName("IX_Books_IsAvailable");

        // Business rule: Unique title + year combination
        builder.HasIndex(b => new { b.Title, b.PublicationYear })
            .IsUnique()
            .HasDatabaseName("UQ_Books_Title_Year");

        // Seed initial data for development and testing
        builder.HasData(
            new
            {
                Id = 1,
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                PublicationYear = PublicationYear.Create(1925),
                Category = Book.BookCategory.Fiction,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = 2,
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                PublicationYear = PublicationYear.Create(1960),
                Category = Book.BookCategory.Fiction,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = 3,
                Title = "1984",
                Author = "George Orwell",
                PublicationYear = PublicationYear.Create(1949),
                Category = Book.BookCategory.Fiction,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = 4,
                Title = "A Brief History of Time",
                Author = "Stephen Hawking",
                PublicationYear = PublicationYear.Create(1988),
                Category = Book.BookCategory.History,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = 5,
                Title = "The Very Hungry Caterpillar",
                Author = "Eric Carle",
                PublicationYear = PublicationYear.Create(1969),
                Category = Book.BookCategory.Child,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Configure audit fields from BaseEntity
        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(b => b.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Ignore domain events collection as it's not persisted
        builder.Ignore(b => b.DomainEvents);
    }
}