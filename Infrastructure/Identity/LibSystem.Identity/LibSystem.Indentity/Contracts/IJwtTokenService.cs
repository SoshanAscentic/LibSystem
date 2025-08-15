using LibSystem.Application.Common.Models;
using LibSystem.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Identity.Contracts
{
    public interface IJwtTokenService
    {
        Task<Result<string>> GenerateTokenAsync(ApplicationUser user, IList<string> roles);
        Task<Result<string>> GenerateRefreshTokenAsync();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<Result<bool>> ValidateTokenAsync(string token);
    }
}