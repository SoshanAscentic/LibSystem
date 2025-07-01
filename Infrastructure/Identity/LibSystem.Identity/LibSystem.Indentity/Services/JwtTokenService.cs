using LibSystem.Application.Common.Models;
using LibSystem.Identity.Configuration;
using LibSystem.Identity.Contracts;
using LibSystem.Identity.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibSystem.Identity.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings jwtSettings;
        private readonly SymmetricSecurityKey key;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            this.jwtSettings = jwtSettings.Value;
            key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.Secret));
        }

        public async Task<Result<string>> GenerateTokenAsync(ApplicationUser user, IList<string> roles)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.UserName ?? user.Email),
                    new(ClaimTypes.Email, user.Email ?? string.Empty),
                    new(ClaimTypes.GivenName, user.FirstName),
                    new(ClaimTypes.Surname, user.LastName),
                    new("FullName", user.FullName),
                    new("IsActive", user.IsActive.ToString()),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Add member ID if linked
                if (user.MemberId.HasValue)
                {
                    claims.Add(new Claim("MemberId", user.MemberId.Value.ToString()));
                }

                // Add roles as claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings.Issuer,
                    audience: jwtSettings.Audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return await Task.FromResult(Result<string>.Success(tokenString));
            }
            catch (Exception)
            {
                return Result<string>.Failure(DomainErrors.Identity.TokenGenerationFailed());
            }
        }

        public async Task<Result<string>> GenerateRefreshTokenAsync()
        {
            try
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                return await Task.FromResult(Result<string>.Success(refreshToken));
            }
            catch (Exception)
            {
                return Result<string>.Failure(DomainErrors.Identity.TokenGenerationFailed());
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false // Don't validate lifetime for expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Result<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                await Task.Run(() => tokenHandler.ValidateToken(token, tokenValidationParameters, out _));
                return Result<bool>.Success(true);
            }
            catch (SecurityTokenExpiredException)
            {
                return Result<bool>.Failure(DomainErrors.Identity.TokenExpired());
            }
            catch
            {
                return Result<bool>.Failure(DomainErrors.Identity.TokenValidationFailed());
            }
        }
    }
}