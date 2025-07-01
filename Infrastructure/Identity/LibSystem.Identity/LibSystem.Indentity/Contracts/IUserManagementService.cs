using LibSystem.Application.Common.Models;
using LibSystem.Identity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Identity.Contracts
{
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