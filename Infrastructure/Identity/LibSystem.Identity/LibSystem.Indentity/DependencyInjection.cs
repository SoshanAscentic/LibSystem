using LibSystem.Identity.Configuration;
using LibSystem.Identity.Context;
using LibSystem.Identity.Contracts;
using LibSystem.Identity.Models;
using LibSystem.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LibSystem.Identity
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure JWT settings
            var jwtSettings = new JwtSettings();
            configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // Add Identity DbContext
            services.AddDbContext<IdentityDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection string is not configured");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
            });

            // Configure Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                IdentityConfiguration.ConfigureIdentityOptions(options);
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

            // Configure JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Configure Authorization
            services.AddAuthorizationBuilder()
                .AddPolicy("RequireAdministratorRole", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.Administrator))
                .AddPolicy("RequireManagementStaffRole", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.ManagementStaff, Constants.ApplicationRoles.Administrator))
                .AddPolicy("RequireStaffRole", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.MinorStaff, Constants.ApplicationRoles.ManagementStaff, Constants.ApplicationRoles.Administrator))
                .AddPolicy("RequireMemberRole", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.Member, Constants.ApplicationRoles.MinorStaff, Constants.ApplicationRoles.ManagementStaff, Constants.ApplicationRoles.Administrator))
                .AddPolicy("BookManagement", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.ManagementStaff, Constants.ApplicationRoles.Administrator))
                .AddPolicy("MemberManagement", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.Administrator))
                .AddPolicy("BorrowingManagement", policy =>
                    policy.RequireRole(Constants.ApplicationRoles.Member, Constants.ApplicationRoles.MinorStaff, Constants.ApplicationRoles.ManagementStaff, Constants.ApplicationRoles.Administrator));

            // Register Identity Services as implementation of Application contracts
            services.AddScoped<LibSystem.Application.Contracts.Identity.IAuthenticationService, AuthenticationService>();
            services.AddScoped<LibSystem.Application.Contracts.Identity.IUserManagementService, UserManagementService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }

        public static async Task<IServiceProvider> InitializeIdentityDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            await identityContext.Database.EnsureCreatedAsync();

            return serviceProvider;
        }
    }
}