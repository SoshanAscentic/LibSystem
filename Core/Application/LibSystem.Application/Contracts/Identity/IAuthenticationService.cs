// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAuthenticationService.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application.Contracts.Identity
{
    using LibSystem.Application.Common.Models;
    using LibSystem.Application.DTOs.Identity;

    public interface IAuthenticationService
    {
        Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);

        Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request);

        Task<Result> LogoutAsync(int userId);

        Task<Result<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request);

        Task<Result> ValidateTokenAsync(string token);

        Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }
}
