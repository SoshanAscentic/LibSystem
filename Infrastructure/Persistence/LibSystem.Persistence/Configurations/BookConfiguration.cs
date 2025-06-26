using LibSystem.Domain.Entities.Books;
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
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {

            builder.ToTable("Books");


            // Configure composite primary key using the domain BookId value object
            builder.HasKey(b => b.Id);

            // Configure BookId value object as a separate property
            builder.Property(b => b.BookId)
                .HasConversion(
                    bookId => bookId.Value,              
                    value => BookId.Create(value))       
                .HasColumnName("BookId")
                .ValueGeneratedOnAdd();                  


            // Title property with validation constraints
            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Title");

            // Author property with validation constraints
            builder.Property(b => b.Author)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Author");

            // PublicationYear value object configuration
            builder.Property(b => b.PublicationYear)
                .HasConversion(
                    year => year.Value,                   
                    value => PublicationYear.Create(value)) 
                .HasColumnName("PublicationYear")
                .IsRequired();

            // Category enum configuration
            builder.Property(b => b.Category)
                .HasConversion<int>()                     
                .HasColumnName("Category")
                .IsRequired();

            // IsAvailable boolean property
            builder.Property(b => b.IsAvailable)
                .HasColumnName("IsAvailable")
                .HasDefaultValue(true)                    
                .IsRequired();



            // Index on Author for author-based searches
            builder.HasIndex(b => b.Author)
                .HasDatabaseName("IX_Books_Author");

            // Index on Category for category-based searches
            builder.HasIndex(b => b.Category)
                .HasDatabaseName("IX_Books_Category");

            // Index on IsAvailable for availability searches
            builder.HasIndex(b => b.IsAvailable)
                .HasDatabaseName("IX_Books_IsAvailable");

            // Composite index for title and year searches (supports duplicate detection)
            builder.HasIndex(b => new { b.Title, b.PublicationYear })
                .HasDatabaseName("IX_Books_Title_PublicationYear");



            // Ensure title and publication year combination is unique (business rule)
            builder.HasIndex(b => new { b.Title, b.PublicationYear })
                .IsUnique()
                .HasDatabaseName("UQ_Books_Title_Year");


            // Seed initial data for development and testing
            builder.HasData(
                new
                {
                    Id = 1,
                    BookId = BookId.Create(1),
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
                    BookId = BookId.Create(2),
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
                    BookId = BookId.Create(3),
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
                    BookId = BookId.Create(4),
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
                    BookId = BookId.Create(5),
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
}
