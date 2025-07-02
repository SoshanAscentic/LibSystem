using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Identity;
using LibSystem.Application.DTOs.Identity;
using LibSystem.Identity.Contracts;
using LibSystem.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LibSystem.Identity.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtTokenService jwtTokenService;
        private readonly IMemberSyncService memberSyncService;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IMemberSyncService memberSyncService,
            ILogger<AuthenticationService> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtTokenService = jwtTokenService;
            this.memberSyncService = memberSyncService;
            this.logger = logger;
        }

        public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidCredentials());
                }

                if (!user.IsActive)
                {
                    logger.LogWarning("Login failed - account deactivated: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.AccountDeactivated());
                }

                var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    logger.LogWarning("Login failed - account locked: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.AccountLocked());
                }

                if (!result.Succeeded)
                {
                    logger.LogWarning("Login failed - invalid credentials: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidCredentials());
                }

                // Get user roles
                var roles = await userManager.GetRolesAsync(user);

                // Ensure domain member sync
                await memberSyncService.SyncUserMemberAsync(user.Id, user.FullName, roles.FirstOrDefault() ?? "Member");

                // Refresh user to get updated MemberId
                user = await userManager.FindByIdAsync(user.Id.ToString());

                // Generate JWT token
                var tokenResult = await jwtTokenService.GenerateTokenAsync(user, roles);
                if (tokenResult.IsFailure)
                {
                    logger.LogError("Token generation failed for user: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(tokenResult.Error);
                }

                var response = new AuthenticationResponse
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? "Member",
                    Token = tokenResult.Value,
                    ExpiresAt = DateTime.UtcNow.AddHours(1), // Should match JWT settings
                    MemberId = user.MemberId
                };

                logger.LogInformation("Login successful for user: {Email}", request.Email);
                return Result<AuthenticationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return Result<AuthenticationResponse>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    logger.LogWarning("Registration failed - user already exists: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.UserAlreadyExists(request.Email));
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Registration failed for email {Email}: {Errors}", request.Email, errors);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.RegistrationFailed(errors));
                }

                // Assign role
                var roleResult = await userManager.AddToRoleAsync(user, request.Role);
                if (!roleResult.Succeeded)
                {
                    logger.LogWarning("Role assignment failed for user {Email}, role {Role}", request.Email, request.Role);
                    // Continue anyway - we can assign role later
                }

                // Create domain member
                var memberResult = await memberSyncService.CreateMemberForUserAsync(user.Id, user.FullName, request.Role);
                if (memberResult.IsSuccess)
                {
                    user.MemberId = memberResult.Value;
                    await userManager.UpdateAsync(user);
                }

                // Get roles for token generation
                var roles = await userManager.GetRolesAsync(user);

                // Generate JWT token
                var tokenResult = await jwtTokenService.GenerateTokenAsync(user, roles);
                if (tokenResult.IsFailure)
                {
                    logger.LogError("Token generation failed for new user: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(tokenResult.Error);
                }

                var response = new AuthenticationResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? request.Role,
                    Token = tokenResult.Value,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    MemberId = user.MemberId
                };

                logger.LogInformation("Registration successful for user: {Email}", request.Email);
                return Result<AuthenticationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return Result<AuthenticationResponse>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> LogoutAsync(int userId)
        {
            try
            {
                logger.LogInformation("Logout for user: {UserId}", userId);
                await signInManager.SignOutAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // TODO: Implement refresh token logic
            await Task.CompletedTask;
            return Result<AuthenticationResponse>.Failure(DomainErrors.General.UnexpectedError());
        }

        public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Result.Failure(DomainErrors.Identity.PasswordChangeFailed(errors));
                }

                logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return Result.Failure(DomainErrors.General.UnexpectedError());
            }
        }
    }
}