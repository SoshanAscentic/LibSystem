using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities.Members
{
    public class ManagementStaff : Staff
    {
        // Parameterless constructor for EF Core
        public ManagementStaff() { }

        public ManagementStaff(string name) : base(name) { }

        public override string GetMemberType() => "Management Staff";
        public override bool CanBorrowBooks() => true;
    }
}
