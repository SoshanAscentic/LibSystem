using LibSystem.Application.Common.Models;
using LibSystem.Identity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Identity.Contracts
{
    public interface IAuthenticationService
    {
        Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);
        Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request);
        Task<Result> LogoutAsync(int userId);
        Task<Result<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<Result> ForgotPasswordAsync(string email);
        Task<Result> ResetPasswordAsync(string email, string token, string newPassword);
    }
}