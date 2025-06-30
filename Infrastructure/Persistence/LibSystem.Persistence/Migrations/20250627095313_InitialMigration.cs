using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PublicationYear = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BorrowingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorrowingId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    BorrowedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingRecords", x => x.Id);
                    table.CheckConstraint("CK_BorrowingRecords_BorrowedAt", "[BorrowedAt] <= GETUTCDATE()");
                    table.CheckConstraint("CK_BorrowingRecords_ReturnedAt", "[ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt]");
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BorrowedBooksCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MemberType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.CheckConstraint("CK_Members_BorrowedBooksCount", "[BorrowedBooksCount] >= 0 AND [BorrowedBooksCount] <= 5");
                    table.CheckConstraint("CK_Members_MemberType", "[MemberType] IN ('RegularMember', 'MinorStaff', 'ManagementStaff')");
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "BookId", "Category", "CreatedAt", "IsAvailable", "PublicationYear", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "F. Scott Fitzgerald", 1, 0, new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2160), true, 1925, "The Great Gatsby", new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2161) },
                    { 2, "Harper Lee", 2, 0, new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2795), true, 1960, "To Kill a Mockingbird", new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2796) },
                    { 3, "George Orwell", 3, 0, new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2798), true, 1949, "1984", new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2798) },
                    { 4, "Stephen Hawking", 4, 1, new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2800), true, 1988, "A Brief History of Time", new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2800) },
                    { 5, "Eric Carle", 5, 2, new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2801), true, 1969, "The Very Hungry Caterpillar", new DateTime(2025, 6, 27, 9, 53, 12, 509, DateTimeKind.Utc).AddTicks(2802) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Author",
                table: "Books",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookId",
                table: "Books",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_Category",
                table: "Books",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsAvailable",
                table: "Books",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "UQ_Books_Title_Year",
                table: "Books",
                columns: new[] { "Title", "PublicationYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingRecords_BookId",
                table: "BorrowingRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingRecords_BorrowedAt",
                table: "BorrowingRecords",
                column: "BorrowedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingRecords_BorrowingId",
                table: "BorrowingRecords",
                column: "BorrowingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingRecords_MemberId",
                table: "BorrowingRecords",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingRecords_ReturnedAt",
                table: "BorrowingRecords",
                column: "ReturnedAt");

            migrationBuilder.CreateIndex(
                name: "UQ_BorrowingRecords_Active",
                table: "BorrowingRecords",
                columns: new[] { "BookId", "MemberId" },
                unique: true,
                filter: "[ReturnedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_BorrowedBooksCount",
                table: "Members",
                column: "BorrowedBooksCount");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberId",
                table: "Members",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberType",
                table: "Members",
                column: "MemberType");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Name",
                table: "Members",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "BorrowingRecords");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
