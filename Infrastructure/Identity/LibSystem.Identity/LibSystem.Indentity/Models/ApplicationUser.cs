using LibSystem.Identity.Constants;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Identity.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters.")]
        [Display(Name = "First Name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters.")]
        [Display(Name = "Last Name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name can only contain letters.")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters.")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        //Navigation property to link with domain Member
        public int? MemberId { get; set; }

        //Helper methof to get member type from roles
        public string GetMemberType(IList<string> roles)
        {
            if (roles.Contains(ApplicationRoles.ManagementStaff))
                return "Management Staff";
            if (roles.Contains(ApplicationRoles.MinorStaff))
                return "Minor Staff";
            if (roles.Contains(ApplicationRoles.Member))
                return "Member";

            return "Unknown";
        }
    }
}