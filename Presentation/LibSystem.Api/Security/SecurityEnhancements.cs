using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

namespace LibSystem.Api.Security
{
    public static class SecurityEnhancements
    {
        public static void AddSecurityEnhancements(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. CSRF Protection
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "CSRF-TOKEN";
                options.Cookie.HttpOnly = false; // Allow JavaScript access for SPA
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // 2. Security Headers
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            // 3. Content Security Policy
            services.AddHeaderPropagation(options =>
            {
                options.Headers.Add("X-Correlation-ID");
            });

            // 4. Enhanced Rate Limiting
            services.AddRateLimiter(options =>
            {
                // General API rate limiting
                options.AddFixedWindowLimiter("api", config =>
                {
                    config.PermitLimit = 100;
                    config.Window = TimeSpan.FromMinutes(1);
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 10;
                });

                // Strict rate limiting for auth endpoints
                options.AddFixedWindowLimiter("auth", config =>
                {
                    config.PermitLimit = 5;
                    config.Window = TimeSpan.FromMinutes(1);
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 2;
                });

                // Per-user rate limiting
                options.AddFixedWindowLimiter("per-user", config =>
                {
                    config.PermitLimit = 200;
                    config.Window = TimeSpan.FromMinutes(1);
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync(
                        "Rate limit exceeded. Please try again later.",
                        cancellationToken: token);
                };
            });

            // 5. Request Size Limits
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            });

            // 6. IP Filtering (if needed)
            services.AddHttpContextAccessor();
        }

        public static void UseSecurityEnhancements(this WebApplication app)
        {
            // 1. Security Headers Middleware
            app.Use(async (context, next) =>
            {
                // Remove server information
                context.Response.Headers.Remove("Server");

                // Security headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

                if (app.Environment.IsProduction())
                {
                    context.Response.Headers.Add("Content-Security-Policy",
                        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; font-src 'self'; img-src 'self' data:;");
                    context.Response.Headers.Add("Strict-Transport-Security",
                        "max-age=31536000; includeSubDomains; preload");
                }

                await next();
            });

            // 2. Rate Limiting
            app.UseRateLimiter();

            // 3. Request Size Limiting
            app.Use(async (context, next) =>
            {
                if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB
                {
                    context.Response.StatusCode = 413;
                    await context.Response.WriteAsync("Request too large");
                    return;
                }
                await next();
            });

            // 4. IP Filtering (if enabled)
            if (app.Configuration.GetValue<bool>("Security:EnableIpFiltering"))
            {
                app.UseIpFiltering();
            }
        }

        private static void UseIpFiltering(this WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                var allowedIps = app.Configuration.GetSection("Security:AllowedIPs").Get<string[]>();

                if (allowedIps != null && allowedIps.Length > 0)
                {
                    if (remoteIp != null && !allowedIps.Contains(remoteIp.ToString()))
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Access denied from this IP address");
                        return;
                    }
                }

                await next();
            });
        }
    }

    // CSRF Token Service
    public interface ICsrfTokenService
    {
        string GenerateToken();
        bool ValidateToken(string token);
    }

    public class CsrfTokenService : ICsrfTokenService
    {
        public string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var bytes = Convert.FromBase64String(token);
                return bytes.Length == 32;
            }
            catch
            {
                return false;
            }
        }
    }

    // Audit Logging Service
    public interface IAuditService
    {
        Task LogSecurityEvent(string eventType, string userId, string details, string ipAddress);
    }

    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> logger;

        public AuditService(ILogger<AuditService> logger)
        {
            this.logger = logger;
        }

        public async Task LogSecurityEvent(string eventType, string userId, string details, string ipAddress)
        {
            logger.LogWarning("SECURITY EVENT: {EventType} | User: {UserId} | IP: {IpAddress} | Details: {Details}",
                eventType, userId, ipAddress, details);

            // Here you could also store in database for audit trail
            await Task.CompletedTask;
        }
    }
}