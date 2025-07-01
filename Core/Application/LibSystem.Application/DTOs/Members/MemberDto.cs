using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.DTOs.Member
{
    public class MemberDto
    {
        public int MemberID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
        public int BorrowedBooksCount { get; set; }
        public bool CanBorrowBooks { get; set; }
        public bool CanViewBooks { get; set; }
        public bool CanViewMembers { get; set; }
        public bool CanManageBooks { get; set; }
    }
}
