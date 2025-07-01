using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.DTOs.Borrowing
{
    public class BorrowBookDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public int MemberID { get; set; }
    }
}
