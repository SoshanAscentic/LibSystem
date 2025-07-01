using LibSystem.Infrastructure.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Infrastructure.Identity.Contracts
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user, IList<string> roles);
        Task<string> RefreshTokenAsync();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<bool> ValidateTokenAsync(string token);
    }
}
