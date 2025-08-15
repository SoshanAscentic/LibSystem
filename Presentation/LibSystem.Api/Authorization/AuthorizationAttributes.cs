using Microsoft.AspNetCore.Authorization;

namespace LibSystem.Api.Authorization
{
    public class RequireMemberAttribute : AuthorizeAttribute
    {
        public RequireMemberAttribute() : base("RequireMemberRole") { }
    }

    public class RequireStaffAttribute : AuthorizeAttribute
    {
        public RequireStaffAttribute() : base("RequireStaffRole") { }
    }

    public class RequireManagementAttribute : AuthorizeAttribute
    {
        public RequireManagementAttribute() : base("RequireManagementStaffRole") { }
    }

    public class RequireAdminAttribute : AuthorizeAttribute
    {
        public RequireAdminAttribute() : base("RequireAdministratorRole") { }
    }

    public class RequireBookManagementAttribute : AuthorizeAttribute
    {
        public RequireBookManagementAttribute() : base("BookManagement") { }
    }

    public class RequireUserManagementAttribute : AuthorizeAttribute
    {
        public RequireUserManagementAttribute() : base("MemberManagement") { }
    }

    public class RequireBorrowingAccessAttribute : AuthorizeAttribute
    {
        public RequireBorrowingAccessAttribute() : base("BorrowingManagement") { }
    }
}