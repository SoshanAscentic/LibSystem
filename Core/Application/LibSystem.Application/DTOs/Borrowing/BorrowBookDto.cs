// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorrowBookDto.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.DTOs.Borrowing
{
    using System.ComponentModel.DataAnnotations;

    public class BorrowBookDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public int MemberID { get; set; }
    }
}
