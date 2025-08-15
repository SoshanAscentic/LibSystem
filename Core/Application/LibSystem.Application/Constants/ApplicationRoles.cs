// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationRoles.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LibSystem.Application.Constants
{
    public static class ApplicationRoles
    {
        public const string Member = "Member";
        public const string MinorStaff = "MinorStaff";
        public const string ManagementStaff = "ManagementStaff";
        public const string Administrator = "Administrator";

        public static readonly string[] AllRoles = new[]
        {
            Member,
            MinorStaff,
            ManagementStaff,
            Administrator,
        };

        public static readonly Dictionary<string, string> RoleDescriptions = new ()
        {
            { Member, "Regular library member with borrowing privileges." },
            { MinorStaff, "Staff member with limited administrative capabilities." },
            { ManagementStaff, "Staff member with management capabilities." },
            { Administrator, "Full administrative access to the system." },
        };
    }
}
