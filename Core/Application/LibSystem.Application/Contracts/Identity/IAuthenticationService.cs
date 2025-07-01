using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Application.Contracts.Identity
{
    public interface IAuthenticationService
    {
        Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);
        Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request);
        Task<Result> LogoutAsync(int userId);
        Task<Result<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }
}