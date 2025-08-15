using AppRoles = LibSystem.Application.Constants.ApplicationRoles; //Reference class

namespace LibSystem.Identity.Constants
{
    public static class ApplicationRoles
    {
        public const string Member = AppRoles.Member;
        public const string MinorStaff = AppRoles.MinorStaff;
        public const string ManagementStaff = AppRoles.ManagementStaff;
        public const string Administrator = AppRoles.Administrator;

        public static readonly string[] AllRoles = AppRoles.AllRoles;
        public static readonly Dictionary<string, string> RoleDescriptions = AppRoles.RoleDescriptions;
    }
}