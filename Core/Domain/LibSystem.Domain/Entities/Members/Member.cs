using LibSystem.Domain.Common;
using LibSystem.Domain.Events;
using LibSystem.Domain.Exceptions;
using LibSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities.Members
{
    public abstract class Member : BaseEntity, IAggregateRoot
    {
        public const int MAX_BORROWED_BOOKS = 5;

        private Name name;
        private int borrowedBooksCount;

        public MemberId MemberId { get; private set; }
        public Name Name
        {
            get => name;
            private set => name = value ?? throw new ArgumentNullException(nameof(Name));
        }
        public int BorrowedBooksCount
        {
            get => borrowedBooksCount;
            private set => borrowedBooksCount = Math.Max(0, value); // Ensure non-negative
        }

        public abstract string GetMemberType();
        public abstract bool CanBorrowBooks();
        public abstract bool CanViewBooks();
        public abstract bool CanViewMembers();
        public abstract bool CanManageBooks();

        protected Member() { }

        protected Member(string name)
        {
            MemberId = MemberId.CreateNew();
            Name = Name.Create(name);
            BorrowedBooksCount = 0;

            // Raise domain event for member creation
            AddDomainEvent(new MemberCreatedEvent(MemberId, Name, GetMemberType()));
        }

        //Factory method for reconstructing existing members from persistence
        public static T Restore<T>(int id, string name, int borrowedBookCounts) where T : Member, new()
        {
            var member = new T();
            member.Id = id;
            member.MemberId = MemberId.Create(id);
            member.name = Name.Create(name);
            member.borrowedBooksCount = borrowedBookCounts;
            return member;
        }

        public virtual void BorrowBook(BookId bookId)
        {
            if (!CanBorrowBooks())
                throw InvalidBorrowingException.MemberCannotBorrow(MemberId.Value, GetMemberType());

            if (BorrowedBooksCount >= MAX_BORROWED_BOOKS)
                throw InvalidBorrowingException.BorrowingLimitExceeded(
                    MemberId.Value, BorrowedBooksCount, MAX_BORROWED_BOOKS);

            BorrowedBooksCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void ReturnBook(BookId bookId)
        {
            if (BorrowedBooksCount <= 0)
                throw InvalidBorrowingException.BookNotBorrowedByMember(bookId.Value, MemberId.Value);

            BorrowedBooksCount--;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasReachedBorrowingLimit()
        {
            return BorrowedBooksCount >= MAX_BORROWED_BOOKS;
        }

        public bool CanBorrowMoreBooks()
        {
            return CanBorrowBooks() && !HasReachedBorrowingLimit();
        }
        public void UpdateName(string newName)
        {
            Name = Name.Create(newName);
            UpdatedAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"{GetMemberType()}: {Name.Value} (ID: {MemberId.Value}) - Borrowed: {BorrowedBooksCount}";
        }
    }
}
}
