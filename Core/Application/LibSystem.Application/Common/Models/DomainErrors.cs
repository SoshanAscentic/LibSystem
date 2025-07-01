using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Common.Models
{
    public static class DomainErrors
    {
        public static class Book
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

        public static class Identity
        {
            // Authentication Errors
            public static Error InvalidCredentials() =>
                Error.Unauthorized("Identity.InvalidCredentials", "Invalid email or password");

            public static Error UserNotFound(string email) =>
                Error.NotFound("Identity.UserNotFound", $"User with email '{email}' was not found");

            public static Error UserNotFoundById(int userId) =>
                Error.NotFound("Identity.UserNotFoundById", $"User with ID {userId} was not found");

            public static Error AccountDeactivated() =>
                Error.Forbidden("Identity.AccountDeactivated", "Your account has been deactivated. Please contact support");

            public static Error AccountLocked() =>
                Error.Forbidden("Identity.AccountLocked", "Your account is locked due to multiple failed login attempts. Please try again later");

            public static Error EmailNotConfirmed() =>
                Error.Forbidden("Identity.EmailNotConfirmed", "Please confirm your email before logging in");

            public static Error InvalidToken() =>
                Error.Unauthorized("Identity.InvalidToken", "Invalid or expired token");

            public static Error TokenExpired() =>
                Error.Unauthorized("Identity.TokenExpired", "Token has expired. Please login again");

            // Registration Errors
            public static Error UserAlreadyExists(string email) =>
                Error.Conflict("Identity.UserAlreadyExists", $"A user with email '{email}' already exists");

            public static Error RegistrationFailed(string details) =>
                Error.Failure("Identity.RegistrationFailed", $"Registration failed: {details}");

            public static Error InvalidRole(string role) =>
                Error.Validation("Identity.InvalidRole", $"Invalid role: {role}. Valid roles are: Member, MinorStaff, ManagementStaff, Administrator");

            // Password Errors
            public static Error WeakPassword() =>
                Error.Validation("Identity.WeakPassword", "Password does not meet security requirements");

            public static Error PasswordMismatch() =>
                Error.Validation("Identity.PasswordMismatch", "Password and confirmation password do not match");

            public static Error CurrentPasswordIncorrect() =>
                Error.Validation("Identity.CurrentPasswordIncorrect", "Current password is incorrect");

            public static Error PasswordChangeFailed(string details) =>
                Error.Failure("Identity.PasswordChangeFailed", $"Password change failed: {details}");

            // User Management Errors
            public static Error EmailAlreadyTaken(string email) =>
                Error.Conflict("Identity.EmailAlreadyTaken", $"Email '{email}' is already taken by another user");

            public static Error UserUpdateFailed(string details) =>
                Error.Failure("Identity.UserUpdateFailed", $"User update failed: {details}");

            public static Error RoleAssignmentFailed(string role, string details) =>
                Error.Failure("Identity.RoleAssignmentFailed", $"Failed to assign role '{role}': {details}");

            public static Error UserNotInRole(int userId, string role) =>
                Error.Validation("Identity.UserNotInRole", $"User {userId} is not assigned to role '{role}'");

            public static Error UserAlreadyInRole(int userId, string role) =>
                Error.Conflict("Identity.UserAlreadyInRole", $"User {userId} is already assigned to role '{role}'");

            public static Error RoleNotFound(string role) =>
                Error.NotFound("Identity.RoleNotFound", $"Role '{role}' does not exist");

            // Member Sync Errors
            public static Error MemberSyncFailed(int userId) =>
                Error.Failure("Identity.MemberSyncFailed", $"Failed to sync domain member for user {userId}");

            public static Error MemberIdNotFound(int userId) =>
                Error.NotFound("Identity.MemberIdNotFound", $"Member ID not found for user {userId}");

            // JWT Token Errors
            public static Error TokenGenerationFailed() =>
                Error.Failure("Identity.TokenGenerationFailed", "Failed to generate authentication token");

            public static Error TokenValidationFailed() =>
                Error.Unauthorized("Identity.TokenValidationFailed", "Token validation failed");

            // Email/Phone Confirmation Errors
            public static Error EmailConfirmationFailed() =>
                Error.Failure("Identity.EmailConfirmationFailed", "Email confirmation failed");

            public static Error InvalidConfirmationToken() =>
                Error.Validation("Identity.InvalidConfirmationToken", "Invalid confirmation token");

            // General Identity Errors
            public static Error InvalidEmailFormat() =>
                Error.Validation("Identity.InvalidEmailFormat", "Invalid email format");

            public static Error InvalidUserId() =>
                Error.Validation("Identity.InvalidUserId", "User ID must be a positive integer");

            public static Error OperationNotAllowed() =>
                Error.Forbidden("Identity.OperationNotAllowed", "This operation is not allowed for the current user");

            public static Error ConcurrencyConflict() =>
                Error.Conflict("Identity.ConcurrencyConflict", "The user data was modified by another process. Please refresh and try again");
        }
    }
}

