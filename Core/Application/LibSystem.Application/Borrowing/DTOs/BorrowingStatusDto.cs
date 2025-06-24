using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Borrowing.DTOs
{
    public class BorrowingStatusDto
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
        public int BorrowedBooksCount { get; set; }
        public bool CanBorrowBooks { get; set; }
        public bool CanBorrowMoreBooks { get; set; }
        public List<BorrowedBookDto> BorrowedBooks { get; set; } = new();
    }

    public class BorrowedBookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public int DaysBorrowed { get; set; }
        public bool IsOverdue { get; set; }
    }
}
