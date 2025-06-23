using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Domain.Entities.Members
{
    public class MinorStaff : Staff
    {
        // Parameterless constructor for EF Core
        public MinorStaff() { }

        public MinorStaff(string name) : base(name) { }

        public override string GetMemberType() => "Minor Staff";
    }
}
