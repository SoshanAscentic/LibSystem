using LibSystem.Api.Common;
using LibSystem.Api.Extensions;
using LibSystem.Application.DTOs.Identity;
using LibSystem.Application.Usecases.Identity.LoginUser;
using LibSystem.Application.Usecases.Identity.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibSystem.Api.Endpoints
{
    public static class SecureAuthenticationEndpoints
    {
        public static void MapSecureAuthenticationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth/secure") // CHANGED: Added /secure
                .WithTags("Secure Authentication")
                .WithOpenApi();

            // POST /api/auth/secure/login - Secure login with httpOnly cookies
            group.MapPost("/login", SecureLogin) // Now: POST /api/auth/secure/login
                .WithName("SecureLogin")
                .WithSummary("Secure user authentication with httpOnly cookies")
                .AllowAnonymous();

            // POST /api/auth/secure/set-tokens - Set httpOnly cookies (internal use)
            group.MapPost("/set-tokens", SetSecureTokens)
                .WithName("SetSecureTokens")
                .RequireAuthorization();

            // GET /api/auth/secure/get-token - Get access token from httpOnly cookie
            group.MapGet("/get-token", GetAccessToken)
                .WithName("GetAccessToken")
                .RequireAuthorization();

            // GET /api/auth/secure/verify - Verify authentication status
            group.MapGet("/verify", VerifyAuthentication)
                .WithName("VerifyAuthentication")
                .RequireAuthorization();

            // POST /api/auth/secure/logout - Secure logout
            group.MapPost("/logout", SecureLogout)
                .WithName("SecureLogout")
                .RequireAuthorization();

            // POST /api/auth/secure/refresh - Refresh tokens
            group.MapPost("/refresh", RefreshTokens)
                .WithName("RefreshTokens")
                .AllowAnonymous();

            // GET /api/auth/secure/me - Get current user (always from server)
            group.MapGet("/me", GetCurrentUserSecure)
                .WithName("GetCurrentUserSecure")
                .WithSummary("Get current user information (server-verified)")
                .RequireAuthorization();
        }

        private static async Task<IResult> SecureLogin(
            [FromBody] LoginRequest request,
            ISender sender,
            HttpContext httpContext)
        {
            var command = new LoginUserCommand(request.Email, request.Password, request.RememberMe);
            var result = await sender.Send(command);

            if (result.IsSuccess)
            {
                // Set httpOnly cookies for tokens
                SetAuthenticationCookies(httpContext, result.Value);

                // Return user info without tokens
                var userResponse = new
                {
                    userId = result.Value.UserId,
                    email = result.Value.Email,
                    fullName = result.Value.FullName,
                    role = result.Value.Role,
                    memberId = result.Value.MemberId
                };

                return Results.Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = userResponse,
                    Message = "User authenticated successfully"
                });
            }

            return result.ToHttpResult();
        }

        private static async Task<IResult> SetSecureTokens(
            [FromBody] TokenData tokenData,
            HttpContext httpContext)
        {
            SetAuthenticationCookies(httpContext, tokenData);
            return Results.Ok(new ApiResponse { Success = true, Message = "Tokens set securely" });
        }

        private static async Task<IResult> GetAccessToken(HttpContext httpContext)
        {
            var accessToken = httpContext.Request.Cookies["access_token"];

            if (string.IsNullOrEmpty(accessToken))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { accessToken }
            });
        }

        private static async Task<IResult> VerifyAuthentication(ClaimsPrincipal user)
        {
            var isAuthenticated = user.Identity?.IsAuthenticated == true;

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { isAuthenticated }
            });
        }

        private static async Task<IResult> SecureLogout(HttpContext httpContext)
        {
            // Clear authentication cookies
            ClearAuthenticationCookies(httpContext);

            return Results.Ok(new ApiResponse
            {
                Success = true,
                Message = "Logged out successfully"
            });
        }

        private static async Task<IResult> RefreshTokens(
            HttpContext httpContext,
            ISender sender)
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            }

            // Call your existing refresh logic here
            // var refreshResult = await sender.Send(new RefreshTokenCommand(refreshToken));

            // For now, return unauthorized to force re-login
            return Results.Unauthorized();
        }

        private static async Task<IResult> GetCurrentUserSecure(
            ClaimsPrincipal user,
            ISender sender)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            // ALWAYS get user from database to ensure current roles/permissions
            var query = new GetUserByIdQuery(userId);
            var result = await sender.Send(query);

            if (result.IsSuccess)
            {
                return Results.Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "User information retrieved successfully"
                });
            }

            return Results.Unauthorized();
        }

        private static void SetAuthenticationCookies(HttpContext httpContext, AuthenticationResponse authResponse)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production
                SameSite = SameSiteMode.Strict,
                Path = "/",
            };

            // Access token - shorter expiration
            var accessTokenOptions = cookieOptions;
            accessTokenOptions.Expires = DateTime.UtcNow.AddMinutes(15); // Short-lived
            httpContext.Response.Cookies.Append("access_token", authResponse.Token, accessTokenOptions);

            // Refresh token - longer expiration
            var refreshTokenOptions = cookieOptions;
            refreshTokenOptions.Expires = DateTime.UtcNow.AddDays(7); // 7 days
            httpContext.Response.Cookies.Append("refresh_token", authResponse.Token, refreshTokenOptions);
        }

        private static void SetAuthenticationCookies(HttpContext httpContext, TokenData tokenData)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            };

            var accessTokenOptions = cookieOptions;
            accessTokenOptions.Expires = DateTimeOffset.FromUnixTimeMilliseconds(tokenData.ExpiresAt).DateTime;
            httpContext.Response.Cookies.Append("access_token", tokenData.AccessToken, accessTokenOptions);

            var refreshTokenOptions = cookieOptions;
            refreshTokenOptions.Expires = DateTime.UtcNow.AddDays(7);
            httpContext.Response.Cookies.Append("refresh_token", tokenData.RefreshToken, refreshTokenOptions);
        }

        private static void ClearAuthenticationCookies(HttpContext httpContext)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1) // Expire immediately
            };

            httpContext.Response.Cookies.Append("access_token", "", cookieOptions);
            httpContext.Response.Cookies.Append("refresh_token", "", cookieOptions);
        }
    }

    public class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public long ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}