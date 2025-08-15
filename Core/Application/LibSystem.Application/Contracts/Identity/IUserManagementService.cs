// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserManagementService.cs" company="Ascentic">
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

    public interface IUserManagementService
    {
        Task<Result<IReadOnlyList<UserDto>>> GetAllUsersAsync();

        Task<Result<UserDto>> GetUserByIdAsync(int userId);

        Task<Result<UserDto>> GetUserByEmailAsync(string email);

        Task<Result<UserDto>> UpdateUserAsync(int userId, UpdateUserRequest request);

        Task<Result> DeactivateUserAsync(int userId);

        Task<Result> ActivateUserAsync(int userId);

        Task<Result> AssignRoleAsync(AssignRoleRequest request);

        Task<Result> RemoveRoleAsync(int userId, string role);

        Task<Result<IReadOnlyList<string>>> GetUserRolesAsync(int userId);
    }
}
