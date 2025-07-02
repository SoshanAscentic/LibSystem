using LibSystem.Application.Constants;
using LibSystem.Application.Contracts.Identity;
using System.Security.Claims;

namespace LibSystem.Api.Authorization
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ClaimsPrincipal? user;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.HttpContext?.User;
        }

        public int? UserId
        {
            get
            {
                var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        public int? MemberId
        {
            get
            {
                var memberIdClaim = user?.FindFirst("MemberId")?.Value;
                return int.TryParse(memberIdClaim, out var memberId) ? memberId : null;
            }
        }

        public string? Email => user?.FindFirst(ClaimTypes.Email)?.Value;

        public string? FullName => user?.FindFirst("FullName")?.Value;

        public IList<string> Roles
        {
            get
            {
                return user?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList() ?? new List<string>();
            }
        }

        public bool IsAuthenticated => user?.Identity?.IsAuthenticated == true;

        public bool IsInRole(string role)
        {
            return user?.IsInRole(role) == true;
        }

        public bool CanManageBooks
        {
            get
            {
                return IsInRole(ApplicationRoles.ManagementStaff) ||
                       IsInRole(ApplicationRoles.Administrator);
            }
        }

        public bool CanManageUsers
        {
            get
            {
                return IsInRole(ApplicationRoles.Administrator);
            }
        }

        public bool CanBorrowBooks
        {
            get
            {
                return IsInRole(ApplicationRoles.Member) ||
                       IsInRole(ApplicationRoles.MinorStaff) ||
                       IsInRole(ApplicationRoles.ManagementStaff) ||
                       IsInRole(ApplicationRoles.Administrator);
            }
        }
    }
}