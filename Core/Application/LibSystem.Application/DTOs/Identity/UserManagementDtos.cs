// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementDtos.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.DTOs.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class UserDto
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new ();

        public int? MemberId { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class AssignRoleRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
