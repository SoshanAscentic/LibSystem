using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities.Members
{
    public class RegularMember : Member
    {
        // Parameterless constructor for EF Core
        public RegularMember() { }

        public RegularMember(string name) : base(name) { }

        public override string GetMemberType() => "Member";
        public override bool CanBorrowBooks() => true;
        public override bool CanViewBooks() => true;
        public override bool CanViewMembers() => true;
    }
}
