using LibSystem.Application.Common.Models;
using LibSystem.Application.DTOs.Identity;
using LibSystem.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibSystem.Identity.Services
{
    public class UserManagementService : Application.Contracts.Identity.IUserManagementService
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
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                var userDtos = new List<UserDto>();
                foreach (var user in users)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
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
                if (userId <= 0)
                {
                    return Result<UserDto>.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Retrieving user with ID: {UserId}", userId);

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", userId);
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                var roles = await userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
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
                logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
                return Result<UserDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Result<UserDto>.Failure(DomainErrors.Identity.InvalidEmailFormat());
                }

                logger.LogInformation("Retrieving user with email: {Email}", email);

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    logger.LogWarning("User not found with email: {Email}", email);
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserNotFound(email));
                }

                var roles = await userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
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
                logger.LogError(ex, "Error retrieving user with email: {Email}", email);
                return Result<UserDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<UserDto>> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            try
            {
                if (userId <= 0)
                {
                    return Result<UserDto>.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Updating user with ID: {UserId}", userId);

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", userId);
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                if (user.Email != request.Email)
                {
                    var existingUser = await userManager.FindByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        return Result<UserDto>.Failure(DomainErrors.Identity.EmailAlreadyTaken(request.Email));
                    }
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
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to update user: {Errors}", errors);
                    return Result<UserDto>.Failure(DomainErrors.Identity.UserUpdateFailed(errors));
                }

                var roles = await userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
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
                logger.LogError(ex, "Error updating user with ID: {UserId}", userId);
                return Result<UserDto>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> DeactivateUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Result.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Deactivating user with ID: {UserId}", userId);

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", userId);
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.UserUpdateFailed(errors));
                }

                logger.LogInformation("Successfully deactivated user: {Email}", user.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deactivating user with ID: {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> ActivateUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return Result.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Activating user with ID: {UserId}", userId);

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", userId);
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.UserUpdateFailed(errors));
                }

                logger.LogInformation("Successfully activated user: {Email}", user.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error activating user with ID: {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> AssignRoleAsync(AssignRoleRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                {
                    return Result.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Assigning role {Role} to user {UserId}", request.Role, request.UserId);

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
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
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
                if (userId <= 0)
                {
                    return Result.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Removing role {Role} from user {UserId}", role, userId);

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
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
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
                if (userId <= 0)
                {
                    return Result<IReadOnlyList<string>>.Failure(DomainErrors.Identity.InvalidUserId());
                }

                logger.LogInformation("Retrieving roles for user {UserId}", userId);

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
                logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
                return Result<IReadOnlyList<string>>.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}