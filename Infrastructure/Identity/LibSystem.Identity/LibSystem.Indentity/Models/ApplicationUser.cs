using LibSystem.Identity.Constants;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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

        /*// Computed property - not mapped to database (ignored in EF configuration)
        public string FullName => $"{FirstName} {LastName}".Trim();*/

        // Simple computed property - EF will handle this as a computed column
        [StringLength(200)]
        public string FullName { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation property to link with domain Member
        public int? MemberId { get; set; }

        // Helper method to get member type from roles
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