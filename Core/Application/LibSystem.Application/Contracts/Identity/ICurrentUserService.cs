// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICurrentUserService.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.Identity
{
    public interface ICurrentUserService
    {
        int? UserId { get; }

        int? MemberId { get; }

        string? Email { get; }

        string? FullName { get; }

        IList<string> Roles { get; }

        bool IsAuthenticated { get; }

        bool CanManageBooks { get; }

        bool CanManageUsers { get; }

        bool CanBorrowBooks { get; }

        bool IsInRole(string role);
    }
}
