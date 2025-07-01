using LibSystem.Application.Common.Models;
using LibSystem.Application.Contracts.Repositories;
using LibSystem.Domain.Entities.Members;
using LibSystem.Infrastructure.Identity.Constants;
using LibSystem.Infrastructure.Identity.Contracts;
using LibSystem.Infrastructure.Identity.DTOs;
using LibSystem.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LibSystem.Identity.Services
{
    public class AuthenticationService : Infrastructure.Identity.Contracts.IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IJwtTokenService jwtTokenService;
        private readonly IMemberRepository memberRepository;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IMemberRepository memberRepository,
            ILogger<AuthenticationService> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtTokenService = jwtTokenService;
            this.memberRepository = memberRepository;
            this.logger = logger;
        }

        public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                logger.LogInformation("Attempting login for user: {Email}", request.Email);

                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    logger.LogWarning("Login failed: User not found for email: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidCredentials());
                }

                if (!user.IsActive)
                {
                    logger.LogWarning("Login failed: User account is deactivated: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.AccountDeactivated());
                }

                var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Login failed: Invalid password for user: {Email}", request.Email);

                    if (result.IsLockedOut)
                    {
                        return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.AccountLocked());
                    }

                    if (result.IsNotAllowed)
                    {
                        return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.EmailNotConfirmed());
                    }

                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidCredentials());
                }

                var roles = await userManager.GetRolesAsync(user);
                var tokenResult = await jwtTokenService.GenerateTokenAsync(user, roles);

                if (!tokenResult.IsSuccess)
                {
                    return Result<AuthenticationResponse>.Failure(tokenResult.Error);
                }

                var response = new AuthenticationResponse
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    Token = tokenResult.Value,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
                    MemberId = user.MemberId
                };

                logger.LogInformation("User successfully logged in: {Email}", request.Email);
                return Result<AuthenticationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return Result<AuthenticationResponse>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                logger.LogInformation("Attempting registration for user: {Email}", request.Email);

                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    logger.LogWarning("Registration failed: User already exists with email: {Email}", request.Email);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.UserAlreadyExists(request.Email));
                }

                // Validate role
                if (!ApplicationRoles.AllRoles.Contains(request.Role))
                {
                    logger.LogWarning("Registration failed: Invalid role specified: {Role}", request.Role);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidRole(request.Role));
                }

                // Create domain member first
                var domainMember = CreateDomainMemberByRole($"{request.FirstName} {request.LastName}", request.Role);
                await memberRepository.AddAsync(domainMember);

                // Note: You'll need to save changes here - adjust based on your UoW pattern
                // await unitOfWork.SaveChangesAsync();

                // Create Identity user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    MemberId = domainMember.Id,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Registration failed: {Errors}", errors);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.RegistrationFailed(errors));
                }

                // Assign role
                var roleResult = await userManager.AddToRoleAsync(user, request.Role);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Role assignment failed during registration: {Errors}", errors);
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.RoleAssignmentFailed(request.Role, errors));
                }

                // Generate token
                var roles = await userManager.GetRolesAsync(user);
                var tokenResult = await jwtTokenService.GenerateTokenAsync(user, roles);

                if (!tokenResult.IsSuccess)
                {
                    return Result<AuthenticationResponse>.Failure(tokenResult.Error);
                }

                var response = new AuthenticationResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = request.Role,
                    Token = tokenResult.Value,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    MemberId = user.MemberId
                };

                logger.LogInformation("User successfully registered: {Email}", request.Email);
                return Result<AuthenticationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during registration for user: {Email}", request.Email);
                return Result<AuthenticationResponse>.Failure(DomainErrors.General.UnexpectedError());
            }
        }

        public async Task<Result> LogoutAsync(int userId)
        {
            try
            {
                logger.LogInformation("User logging out: {UserId}", userId);
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
            try
            {
                var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
                if (principal == null)
                {
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidToken());
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.InvalidToken());
                }

                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                {
                    return Result<AuthenticationResponse>.Failure(DomainErrors.Identity.UserNotFoundById(userId));
                }

                var roles = await userManager.GetRolesAsync(user);
                var tokenResult = await jwtTokenService.GenerateTokenAsync(user, roles);

                if (!tokenResult.IsSuccess)
                {
                    return Result<AuthenticationResponse>.Failure(tokenResult.Error);
                }

                var response = new AuthenticationResponse
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    Token = tokenResult.Value,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    MemberId = user.MemberId
                };

                return Result<AuthenticationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during token refresh");
                return Result<AuthenticationResponse>.Failure(DomainErrors.General.UnexpectedError());
            }
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
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, errors);

                    // Check for specific error types
                    if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                    {
                        return Result.Failure(DomainErrors.Identity.CurrentPasswordIncorrect());
                    }

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

        public async Task<Result> ForgotPasswordAsync(string email)
        {
            // Implementation for forgot password functionality
            await Task.CompletedTask;
            return Result.Failure(DomainErrors.General.UnexpectedError());
        }

        public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
        {
            // Implementation for password reset functionality
            await Task.CompletedTask;
            return Result.Failure(DomainErrors.General.UnexpectedError());
        }

        private static Member CreateDomainMemberByRole(string name, string role)
        {
            return role switch
            {
                ApplicationRoles.Member => new RegularMember(name),
                ApplicationRoles.MinorStaff => new MinorStaff(name),
                ApplicationRoles.ManagementStaff => new ManagementStaff(name),
                _ => new RegularMember(name)
            };
        }
    }
}