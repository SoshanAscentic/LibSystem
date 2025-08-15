using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Identity;
using LibSystem.Application.DTOs.Identity;
using LibSystem.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibSystem.Identity.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ILogger<UserManagementService> logger;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UserManagementService> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
        }

        public async Task<Result<IReadOnlyList<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                logger.LogInformation("Retrieving all users");

                var users = await userManager.Users
                    .OrderBy(u => u.Email)
                    .ToListAsync();

                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FullName = user.FullName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Roles = roles.ToList(),
                        MemberId = user.MemberId
                    });
                }

                logger.LogInformation("Successfully retrieved {Count} users", userDtos.Count);
                return Result<IReadOnlyList<UserDto>>.Success(userDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all users");
                return Result<IReadOnlyList<UserDto>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                logger.LogInformation("Retrieving user by ID: {UserId}", userId);

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    logger.LogWarning("User not found: {UserId}", userId);
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                var roles = await userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList(),
                    MemberId = user.MemberId
                };

                logger.LogInformation("Successfully retrieved user: {Email}", user.Email);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return Result<UserDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                logger.LogInformation("Retrieving user by email: {Email}", email);

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    logger.LogWarning("User not found: {Email}", email);
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserNotFound(email));
                }

                var roles = await userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList(),
                    MemberId = user.MemberId
                };

                logger.LogInformation("Successfully retrieved user: {Email}", email);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return Result<UserDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<UserDto>> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            try
            {
                logger.LogInformation("Updating user: {UserId}", userId);

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                // Check if email is already taken by another user
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return Result<UserDto>.Failure(DomainErrors.Identity.EmailAlreadyTaken(request.Email));
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.UserName = request.Email;
                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserUpdateFailed(errors));
                }

                var roles = await userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList(),
                    MemberId = user.MemberId
                };

                logger.LogInformation("Successfully updated user: {Email}", user.Email);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user: {UserId}", userId);
                return Result<UserDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.UserUpdateFailed(errors));
                }

                logger.LogInformation("Successfully deactivated user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> ActivateUserAsync(int userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.UserUpdateFailed(errors));
                }

                logger.LogInformation("Successfully activated user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error activating user: {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> AssignRoleAsync(AssignRoleRequest request)
        {
            try
            {
                var user = await userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(request.UserId));
                }

                var roleExists = await roleManager.RoleExistsAsync(request.Role);
                if (!roleExists)
                {
                    return Result.Failure(DomainErrors.Identity.RoleNotFound(request.Role));
                }

                var isInRole = await userManager.IsInRoleAsync(user, request.Role);
                if (isInRole)
                {
                    return Result.Failure(DomainErrors.Identity.UserAlreadyInRole(request.UserId, request.Role));
                }

                var result = await userManager.AddToRoleAsync(user, request.Role);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.RoleAssignmentFailed(request.Role, errors));
                }

                logger.LogInformation("Successfully assigned role {Role} to user {UserId}", request.Role, request.UserId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error assigning role {Role} to user {UserId}", request.Role, request.UserId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> RemoveRoleAsync(int userId, string role)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                var isInRole = await userManager.IsInRoleAsync(user, role);
                if (!isInRole)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotInRole(userId, role));
                }

                var result = await userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.RoleAssignmentFailed(role, errors));
                }

                logger.LogInformation("Successfully removed role {Role} from user {UserId}", role, userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing role {Role} from user {UserId}", role, userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<IReadOnlyList<string>>> GetUserRolesAsync(int userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result<IReadOnlyList<string>>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                var roles = await userManager.GetRolesAsync(user);
                return Result<IReadOnlyList<string>>.Success(roles.ToList());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return Result<IReadOnlyList<string>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}