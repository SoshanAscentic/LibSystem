using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        bool IsInRole(string role);
        bool CanManageBooks { get; }
        bool CanManageUsers { get; }
        bool CanBorrowBooks { get; }
    }
}