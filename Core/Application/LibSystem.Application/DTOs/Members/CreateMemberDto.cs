// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateMemberDto.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.DTOs.Members
{
    using System.ComponentModel.DataAnnotations;

    public class CreateMemberDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 2)]
        public int MemberType { get; set; }
    }
}
