using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities
{
    public abstract class Member
    {
        private string name;

        public string Name
        {
            get => name ?? string.Empty;
            set => name = value;
        }

        public int MemberID { get; set; } // EF Core will auto-generate this
        public int BorrowedBooksCount { get; set; } = 0;

        // Parameterless constructor for EF Core
        protected Member() { }

        protected Member(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            this.name = name;
        }

        public abstract string GetMemberType();
        public virtual bool CanBorrowBooks() => false;
        public virtual bool CanViewBooks() => false;
        public virtual bool CanViewMembers() => false;
        public virtual bool CanManageBooks() => false;

        public override string ToString()
        {
            return $"Name: {Name}, ID: {MemberID}, Type: {GetMemberType()}, Borrowed Books: {BorrowedBooksCount}";
        }
    }
}
