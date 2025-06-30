using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Common.Models
{
    public static class DomainErrors
    {
        public static Error NotFound(int bookId) =>
                Error.NotFound("Book.NotFound", $"Book with ID {bookId} was not found");

        public static Error AlreadyExists(string title, int year) =>
            Error.Conflict("Book.AlreadyExists", $"A book with title '{title}' and publication year {year} already exists");

        public static Error NotAvailable(int bookId) =>
            Error.Conflict("Book.NotAvailable", $"Book with ID {bookId} is not available for borrowing");

        public static Error CurrentlyBorrowed(string title) =>
            Error.Conflict("Book.CurrentlyBorrowed", $"Cannot delete book '{title}' as it is currently borrowed");

        public static Error InvalidTitle() =>
            Error.Validation("Book.InvalidTitle", "Book title cannot be empty and must not exceed 200 characters");

        public static Error InvalidAuthor() =>
            Error.Validation("Book.InvalidAuthor", "Book author cannot be empty and must not exceed 100 characters");

        public static Error InvalidPublicationYear() =>
            Error.Validation("Book.InvalidPublicationYear", "Publication year must be between 1450 and current year");

        public static Error InvalidCategory() =>
            Error.Validation("Book.InvalidCategory", "Book category must be Fiction, History, or Child");
    }

    public static class Member
    {
        public static Error NotFound(int memberId) =>
            Error.NotFound("Member.NotFound", $"Member with ID {memberId} was not found");

        public static Error CannotBorrow(int memberId, string memberType) =>
            Error.Forbidden("Member.CannotBorrow", $"Member {memberId} of type '{memberType}' does not have borrowing permissions");

        public static Error BorrowingLimitExceeded(int memberId, int currentCount, int maxLimit) =>
            Error.Conflict("Member.BorrowingLimitExceeded", $"Member {memberId} has reached the borrowing limit. Current: {currentCount}, Max: {maxLimit}");

        public static Error InvalidName() =>
            Error.Validation("Member.InvalidName", "Member name cannot be empty and must not exceed 100 characters");

        public static Error InvalidMemberType() =>
            Error.Validation("Member.InvalidMemberType", "Member type must be 0 (Member), 1 (Minor Staff), or 2 (Management Staff)");
    }

    public static class Borrowing
    {
        public static Error BookNotBorrowedByMember(int bookId, int memberId) =>
            Error.NotFound("Borrowing.NotFound", $"No active borrowing found for book {bookId} by member {memberId}");

        public static Error BookAlreadyReturned(int bookId) =>
            Error.Conflict("Borrowing.AlreadyReturned", $"Book with ID {bookId} is not currently borrowed");

        public static Error InvalidBorrowingOperation() =>
            Error.Validation("Borrowing.InvalidOperation", "Invalid borrowing operation");
    }

    public static class General
    {
        public static Error InvalidId(string entityName) =>
            Error.Validation("General.InvalidId", $"{entityName} ID must be a positive integer");

        public static Error DatabaseError() =>
            Error.Failure("General.DatabaseError", "A database error occurred. Please try again");

        public static Error UnexpectedError() =>
            Error.Failure("General.UnexpectedError", "An unexpected error occurred. Please try again later");
    }
}
}
