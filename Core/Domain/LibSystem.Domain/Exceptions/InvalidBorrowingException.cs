using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Exceptions
{
    public sealed class InvalidBorrowingException : DomainException
    {
        public InvalidBorrowingException(string message) : base(message)
        {
        }

        public static InvalidBorrowingException BookNotAvailable(int bookId) =>
            new($"Book with ID {bookId} is not available for borrowing.");

        public static InvalidBorrowingException BookAlreadyReturned(int bookId) =>
            new($"Book with ID {bookId} is not currently borrowed.");

        public static InvalidBorrowingException BorrowingLimitExceeded(int memberId, int currentCount, int maxLimit) =>
            new($"Member {memberId} has reached the borrowing limit. Current: {currentCount}, Max: {maxLimit}");

        public static InvalidBorrowingException MemberCannotBorrow(int memberId, string memberType) =>
            new($"Member {memberId} of type '{memberType}' does not have borrowing permissions.");

        public static InvalidBorrowingException BookNotBorrowedByMember(int bookId, int memberId) =>
            new($"Book {bookId} is not currently borrowed by member {memberId}.");
    }
}
